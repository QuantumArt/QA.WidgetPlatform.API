using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QA.DotNetCore.Engine.Abstractions;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.DotNetCore.Engine.Abstractions.Wildcard;
using QA.DotNetCore.Engine.QpData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace QA.WidgetPlatform.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SiteController : ControllerBase
    {
        private readonly ILogger<SiteController> _logger;
        private readonly IAbstractItemStorageProvider _abstractItemStorageProvider;

        public SiteController(ILogger<SiteController> logger, IAbstractItemStorageProvider abstractItemStorageProvider)
        {
            _logger = logger;
            _abstractItemStorageProvider = abstractItemStorageProvider;
        }

        /// <summary>
        /// Получение структуры страниц сайта
        /// </summary>
        /// <param name="dnsName">Доменное имя сайта</param>
        /// <param name="targeting">Словарь значений таргетирования</param>
        /// <returns></returns>
        [HttpGet("structure")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public SiteNode Structure([Required] [FromQuery] string dnsName, [Bind(Prefix = "t")] [FromQuery] IDictionary<string, string> targeting)
        {
            var storage = _abstractItemStorageProvider.Get();

            var targetingFilter = CreateMtsFilter(targeting);

            #region копипаста из QA.DotNetCore.Engine.Abstractions.AbstractItemStorage, плохо спроектирован интерфейс IStartPage, к которому идёт привязка логики по стартовой странице

            var bindings = new List<string>();
            Dictionary<string, UniversalAbstractItem> startPageByDnsPatternMappings = new Dictionary<string, UniversalAbstractItem>();
            foreach (var rootChild in storage.Root.GetChildren(targetingFilter).OfType<UniversalAbstractItem>())
            {
                if (rootChild.UntypedFields.Keys.Any(k => k.ToLowerInvariant() == "bindings"))
                {
                    var key = rootChild.UntypedFields.Keys.First(k => k.ToLowerInvariant() == "bindings");

                    if (rootChild.UntypedFields[key] is string dnsString)
                    {
                        var dnsArray = dnsString
                            .Split(new char[] { '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(_ => _.Trim())
                            .ToArray();

                        bindings.AddRange(dnsArray);

                        Array.ForEach(dnsArray, x => startPageByDnsPatternMappings[x] = rootChild);
                    }
                }
            }

            var matcher = new WildcardMatcher(WildcardMatchingOption.FullMatch, bindings);
            var pattern = matcher.MatchLongest(dnsName);
            var startPage = pattern != null ? startPageByDnsPatternMappings[pattern] : null;
            #endregion

            if (startPage == null)
                throw new System.Exception("404");//todo

            return new SiteNode(startPage, new OnlyPagesFilter() + targetingFilter);
        }

        /// <summary>
        /// Получение детальной информации по странице или виджету
        /// </summary>
        /// <param name="nodeId">id страницы или виджета</param>
        /// <returns></returns>
        [HttpGet("node/{nodeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public SiteNodeDetails Node(int nodeId)
        {
            var storage = _abstractItemStorageProvider.Get();

            var node = storage.Get(nodeId);

            if (node == null || !(node is UniversalAbstractItem))
                throw new System.Exception("404");//todo


            return new SiteNodeDetails(node as UniversalAbstractItem);
        }

        /// <summary>
        /// Получение виджетов для страницы, сгруппированных по зонам
        /// </summary>
        /// <param name="pageId">id страницы</param>
        /// <param name="targeting">Словарь значений таргетирования</param>
        /// <param name="zones">Список виджетных зон (если не передавать, то поиск виджетов не будет производиться для рекурсивных и глобальных зон)</param>
        /// <returns></returns>
        [HttpGet("widgets/{pageId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IDictionary<string, WidgetDetails[]> WidgetsForPage(int pageId, [Bind(Prefix = "t")] [FromQuery] IDictionary<string, string> targeting, [FromQuery] string[] zones)
        {
            var storage = _abstractItemStorageProvider.Get();
            var page = storage.Get(pageId);

            if (page == null)
                throw new System.Exception("404");//todo

            var targetingFilter = new OnlyWidgetsFilter() + CreateMtsFilter(targeting);

            //виджеты, инфу о которых мы вернем в этой методе, могут быть не только у текущей страницы, т.к.
            //если запрашиваются виджеты в рекурсивных зонах, то они могут быть дочерними не для текущей страницы, а для какой-то из её родительских страниц
            //если запрашиваются виджеты в глобальных зонах, то они должны быть дочерними для стартовой страницы

            var result = new Dictionary<string, WidgetDetails[]>();
            if (zones != null && zones.Any())
            {
                var atLeastOneRecursiveOrGlobalZone = zones.Any(z => ZoneIsRecursive(z) || ZoneIsGlobal(z));

                if (atLeastOneRecursiveOrGlobalZone)
                {
                    //пройдём по всей иерархии вверх до стартовой страницы, чтобы достать оттуда виджеты в рекурсивных или глобальных зонах
                    var currentPage = page;
                    while (currentPage != null)
                    {
                        var zonesToSearch = currentPage.Id != page.Id ? 
                            (PageIsStart(currentPage) ? 
                                zones.Where(z => ZoneIsRecursive(z) || ZoneIsGlobal(z)).ToArray() : 
                                zones.Where(z => ZoneIsRecursive(z)).ToArray()) :
                            zones;

                        var widgetGroups = ChildWidgetsGroupedByZone(currentPage, targetingFilter, zonesToSearch);
                        foreach (var kvp in widgetGroups)
                        {
                            if (result.ContainsKey(kvp.Key))
                            {
                                //редкая ситуация, когда виджеты в одной и той же рекурсивной зоне есть на разных уровнях дерева сайта
                                result[kvp.Key] = result[kvp.Key].Union(kvp.Value).OrderBy(w => w.SortOrder).ToArray();
                            }
                            else
                            {
                                result[kvp.Key] = kvp.Value;
                            }
                        }

                        currentPage = currentPage.Parent;
                    }
                }
                else
                {
                    result = ChildWidgetsGroupedByZone(page, targetingFilter, zones);
                }
            }
            else
            {
                //если зоны явно не переданы в этот метод - возвращаем виджеты у текущей страницы во всех зонах
                result = ChildWidgetsGroupedByZone(page, targetingFilter);
            }

            return result;
        }

        private Dictionary<string, WidgetDetails[]> ChildWidgetsGroupedByZone(IAbstractItem item, ITargetingFilter filter, IEnumerable<string> zones = null)
        {
            return item
                    .GetChildren(filter)
                    .OrderBy(ai => ai.SortOrder)
                    .Cast<UniversalAbstractItem>()
                    .Where(w => w.UntypedFields.ContainsKey("ZONENAME") && w.UntypedFields["ZONENAME"] != null  //по идее в библиотеках должен возвращаться UniversalAbstractWidget для виджетов, знание о том, как из UniversalAbstractItem достать виджетную зону должно быть там
                        && (zones == null || zones.Contains(w.UntypedFields["ZONENAME"].ToString())))
                    .GroupBy(w => w.UntypedFields["ZONENAME"].ToString())
                    .ToDictionary(g => g.Key, g => g.Select(w => new WidgetDetails(w, abstractItem => ChildWidgetsGroupedByZone(abstractItem, filter))).ToArray());
        }

        private static MtsRegionFilter CreateMtsFilter(IDictionary<string, string> currentTargeting)
        {
            var idList = new List<int>();
            if (currentTargeting.ContainsKey("region") && int.TryParse(currentTargeting["region"], out int regionId))
            {
                idList.Add(regionId);

                //TODO: здесь надо добавить id всех предков этого региона в дереве регионов МТС
            }

            return new MtsRegionFilter(idList);
        }

        private static bool PageIsStart(IAbstractItem ai)
        {
            return ai.Parent != null && ai.Parent.Parent == null; // дурацкое определение для стартовой страницы, но сейчас это самое простое
        }

        #region копипаста из QA.DotNetCore.Engine.Widgets.ComponentExtensions
        private static bool ZoneIsRecursive(string zoneName)
        {
            return zoneName.StartsWith("Recursive");
        }

        private static bool ZoneIsGlobal(string zoneName)
        {
            return zoneName.StartsWith("Site");
        }
        #endregion
    }
}
