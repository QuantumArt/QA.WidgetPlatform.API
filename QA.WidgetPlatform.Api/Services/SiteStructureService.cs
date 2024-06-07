using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.DotNetCore.Engine.Abstractions;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.DotNetCore.Engine.QpData;
using QA.DotNetCore.Engine.Targeting.Filters;
using QA.DotNetCore.Engine.Widgets;
using QA.WidgetPlatform.Api.Application.Exceptions;
using QA.WidgetPlatform.Api.Models;
using QA.WidgetPlatform.Api.Services.Abstract;

namespace QA.WidgetPlatform.Api.Services
{
    internal class SiteStructureService : ISiteStructureService
    {
        private readonly IAbstractItemStorageProvider _abstractItemStorageProvider;
        private readonly ITargetingFilterAccessor _targetingFiltersAccessor;
        private readonly ITargetingContextUpdater _targetingUpdater;
        private readonly ILogger<SiteStructureService> _logger;
        private readonly FieldsSettings _fieldsSettings;

        public SiteStructureService(IAbstractItemStorageProvider abstractItemStorageProvider,
            ITargetingFilterAccessor targetingFiltersAccessor,
            ITargetingContextUpdater targetingUpdater,
            ILogger<SiteStructureService> logger,
            IOptions<FieldsSettings> fieldsSettings)
        {
            _abstractItemStorageProvider = abstractItemStorageProvider;
            _targetingFiltersAccessor = targetingFiltersAccessor;
            _targetingUpdater = targetingUpdater;
            _logger = logger;
            _fieldsSettings = fieldsSettings.Value;
        }

        public void Warmup()
        {
            _logger.LogInformation("Warmup start");
            _abstractItemStorageProvider.Get();
            _logger.LogInformation("Warmup end");
        }

        /// <summary>
        /// Получение структуры страниц сайта
        /// </summary>
        /// <param name="dnsName">Доменное имя сайта</param>
        /// <param name="targeting">Словарь значений таргетирования</param>
        /// <param name="fields">Поля деталей к выдаче. Если пусто, то детали выдаваться не будут</param>
        /// <param name="deep">Глубина страуктуры, где 0 - это корневой элемент</param>
        /// <param name="fillDefinitionDetails">Заполнять дополнительные поля из дефинишена</param>
        /// <returns></returns>
        [HttpGet("structure")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public SiteNode Structure(
            string dnsName,
            IDictionary<string, string> targeting,
            string[] fields,
            int? deep, bool fillDefinitionDetails = false)
        {
            var storage = _abstractItemStorageProvider.Get();
            _targetingUpdater.Update(targeting).Wait();
            var startPageFilter = _targetingFiltersAccessor.Get(TargetingDestination.Structure);

            var startPage = storage.GetStartPage<UniversalPage>(dnsName, startPageFilter);
            if (startPage == null)
                throw new StatusCodeException(System.Net.HttpStatusCode.NotFound);

            var pagesFilters = new OnlyPagesFilter().AddFilter(startPageFilter);
            var includeFields = new HashSet<string>(fields, StringComparer.OrdinalIgnoreCase);

            return new SiteNode(startPage, pagesFilters, includeFields, deep, fillDefinitionDetails);
        }

        /// <summary>
        /// Получение массива нод, удовлетворяющих переданным фильтрам
        /// </summary>
        /// <param name="dnsName">Доменное имя сайта</param>
        /// <param name="targeting">Словарь значений таргетирования</param>
        /// <param name="fields">Поля деталей к выдаче. Если пусто, то будут выведены все детали</param>
        /// <returns></returns>
        public IEnumerable<SimpleSiteNodeDetails> Details(string dnsName,
            IDictionary<string, string> targeting,
            string[] fields)
        {
            var storage = _abstractItemStorageProvider.Get();
            _targetingUpdater.Update(targeting).Wait();

            var startPageFilter = _targetingFiltersAccessor.Get(TargetingDestination.Structure);
            var nodeFilter = _targetingFiltersAccessor.Get(TargetingDestination.Nodes);

            var nodes = storage.GetNodes<UniversalAbstractItem>(dnsName, startPageFilter, nodeFilter);

            if (nodes == null || !nodes.Any())
                throw new StatusCodeException(System.Net.HttpStatusCode.NotFound);

            return nodes
                .Select(x => new SimpleSiteNodeDetails(x, fields));
        }

        /// <summary>
        /// Получение детальной информации по странице или виджету
        /// </summary>
        /// <param name="nodeId">id страницы или виджета</param>
        /// <returns></returns>
        public SiteNodeDetails Node(int nodeId)
        {
            var storage = _abstractItemStorageProvider.Get();

            var node = storage.Get<UniversalAbstractItem>(nodeId);

            if (node == null)
            {
                throw new StatusCodeException(System.Net.HttpStatusCode.NotFound);
            }
            else
            {
                return new SiteNodeDetails(node, includeNullFields: _fieldsSettings.IncludeNullFieldsInNode);
            }
        }

        /// <summary>
        /// Получение виджетов для страницы или виджета, сгруппированных по зонам
        /// </summary>
        /// <param name="abstractItemId">id страницы или виджета</param>
        /// <param name="targeting">Словарь значений таргетирования</param>
        /// <param name="zones">Список виджетных зон (если не передавать, то поиск виджетов не будет производиться для рекурсивных и глобальных зон)</param>
        /// <param name="fields">Поля деталей к выдаче. Если пусто, то будут выведены все детали</param>
        /// <param name="fillDefinitionDetails">Заполнять дополнительные поля из дефинишена</param>
        /// <returns></returns>
        public IDictionary<string, WidgetDetails[]> WidgetsForNode(int abstractItemId,
            IDictionary<string, string> targeting, string[] zones, string[] fields, bool fillDefinitionDetails = false)
        {
            var storage = _abstractItemStorageProvider.Get();
            var abstractItem = storage.Get<UniversalAbstractItem>(abstractItemId);
            var isPage = abstractItem is UniversalPage;

            if (abstractItem == null)
                throw new StatusCodeException(System.Net.HttpStatusCode.NotFound);

            _targetingUpdater.Update(targeting).Wait();
            var filter = _targetingFiltersAccessor.Get(TargetingDestination.Nodes);

            var targetingFilter =
                new OnlyWidgetsFilter().AddFilter(filter);

            //виджеты, инфу о которых мы вернем в этой методе, могут быть не только у текущей страницы, т.к.
            //если запрашиваются виджеты в рекурсивных зонах, то они могут быть дочерними не для текущей страницы, а для какой-то из её родительских страниц
            //если запрашиваются виджеты в глобальных зонах, то они должны быть дочерними для стартовой страницы

            var result = new Dictionary<string, WidgetDetails[]>();
            if (zones != null && zones.Any())
            {
                var atLeastOneRecursiveOrGlobalZone = zones.Any(z => ZoneIsRecursive(z) || ZoneIsGlobal(z));

                if (isPage && atLeastOneRecursiveOrGlobalZone)
                {
                    //пройдём по всей иерархии вверх до стартовой страницы, чтобы достать оттуда виджеты в рекурсивных или глобальных зонах
                    IAbstractItem currentPage = abstractItem;
                    while (currentPage != null)
                    {
                        var zonesToSearch = currentPage.Id != abstractItem.Id
                            ? (PageContainsGlobalWidgets(currentPage)
                                ? zones.Where(z => ZoneIsRecursive(z) || ZoneIsGlobal(z)).ToArray()
                                : zones.Where(z => ZoneIsRecursive(z)).ToArray())
                            : zones;

                        var widgetGroups = ChildWidgetsGroupedByZone(
                            currentPage, targetingFilter, fillDefinitionDetails, fields, zonesToSearch
                        );
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
                    result = ChildWidgetsGroupedByZone(abstractItem, targetingFilter, fillDefinitionDetails, fields, zones);
                }
            }
            else
            {
                //если зоны явно не переданы в этот метод - возвращаем виджеты у текущей страницы во всех зонах
                result = ChildWidgetsGroupedByZone(abstractItem, targetingFilter, fillDefinitionDetails, fields);
            }

            return result;
        }

        private Dictionary<string, WidgetDetails[]> ChildWidgetsGroupedByZone(
            IAbstractItem item,
            ITargetingFilter filter,
            bool fillDefinitionDetails,
            IEnumerable<string>? fields = null,
            IEnumerable<string>? zones = null
        )
        {
            return item
                .GetChildren<UniversalWidget>(filter)
                .OrderBy(ai => ai.SortOrder)
                .Where(w => w.ZoneName != null && (zones == null || zones.Contains(w.ZoneName)))
                .GroupBy(w => w.ZoneName)
                .ToDictionary(
                    k => k.Key,
                    v => v.Select(w =>
                        new WidgetDetails(w,
                            abstractItem => ChildWidgetsGroupedByZone(
                                abstractItem, filter, fillDefinitionDetails, fields
                            ),
                            fields,
                            fillDefinitionDetails
                        )
                    ).ToArray()
                );
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
    }
}
