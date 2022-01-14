using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QA.DotNetCore.Engine.Abstractions;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.DotNetCore.Engine.QpData;
using QA.DotNetCore.Engine.Widgets;
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

            var startPage = storage.GetStartPage<UniversalPage>(dnsName, targetingFilter);
            if (startPage == null)
                throw new StatusCodeException(System.Net.HttpStatusCode.NotFound);

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

            var node = storage.Get<UniversalPage>(nodeId);

            if (node == null)
                throw new StatusCodeException(System.Net.HttpStatusCode.NotFound);


            return new SiteNodeDetails(node);
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
            var page = storage.Get<UniversalPage>(pageId);

            if (page == null)
                throw new StatusCodeException(System.Net.HttpStatusCode.NotFound);

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
                    IAbstractItem currentPage = page;
                    while (currentPage != null)
                    {
                        var zonesToSearch = currentPage.Id != page.Id ? 
                            (PageContainsGlobalWidgets(currentPage) ? 
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

        /// <summary>
        /// Прогрев
        /// </summary>
        /// <returns>id корневой страницы</returns>
        [HttpGet("warmup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public int WarmUp()
        {
            var storage = _abstractItemStorageProvider.Get();
            return storage.Root.Id;
        }

        private Dictionary<string, WidgetDetails[]> ChildWidgetsGroupedByZone(IAbstractItem item, ITargetingFilter filter, IEnumerable<string> zones = null)
        {
            return item
                    .GetChildren<UniversalWidget>(filter)
                    .OrderBy(ai => ai.SortOrder)
                    .Where(w => w.ZoneName != null && (zones == null || zones.Contains(w.ZoneName)))
                    .GroupBy(w => w.ZoneName)
                    .ToDictionary(g => g.Key, g => g.Select(w => new WidgetDetails(w, abstractItem => ChildWidgetsGroupedByZone(abstractItem, filter))).ToArray());
        }

        private static bool PageContainsGlobalWidgets(IAbstractItem ai)
        {
            // дурацкое определение для стартовой страницы, но сейчас это самое простое, возможно ничего лучше не придумать
            return ai.Parent != null && ai.Parent.Parent == null; 
        }

        private static bool ZoneIsRecursive(string zoneName)
        {
            return WidgetZoneTypeQualifier.QualifyZone(zoneName) == WidgetZoneType.Recursive;
        }

        private static bool ZoneIsGlobal(string zoneName)
        {
            return WidgetZoneTypeQualifier.QualifyZone(zoneName) == WidgetZoneType.Global;
        }

        #region единственная МТС-ная специфика

        private static MtsRegionFilter CreateMtsFilter(IDictionary<string, string> currentTargeting)
        {
            //для работы мтс-ного сайта нам нужно таргетироваться по региону
            //ожидается, что в словаре значений таргетирования к нам придёт запись с ключом "region"
            //и в значении будет список id: id текущего региона и всех родительских через запятую
            List<int> idList;
            if (currentTargeting.ContainsKey("region"))
            {
                idList = currentTargeting["region"]
                    .Split(',')
                    .Select(t => int.TryParse(t, out int parsed) ? parsed : 0)
                    .Where(id => id > 0)
                    .ToList();
            }
            else
            {
                idList = new List<int>();
            }

            return new MtsRegionFilter(idList);
        }

        #endregion
    }
}
