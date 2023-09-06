using System.Collections.Generic;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.WidgetPlatform.Api.TargetingFilters;

namespace QA.WidgetPlatform.Api
{
    public class RegionTargetingFiltersFactory : ITargetingFiltersFactory
    {
        private readonly ILogger<RegionTargetingFiltersFactory> _logger;

        public RegionTargetingFiltersFactory(ILogger<RegionTargetingFiltersFactory> logger)
        {
            _logger = logger;
        }

        public ITargetingFilter StructureFilter(IDictionary<string, string> targeting)
        {
            return CreateRegionFilter(targeting);
        }

        public ITargetingFilter FlattenNodesFilter(IDictionary<string, string> targeting)
        {
            return CreateRegionFilter(targeting);
        }

        private ITargetingFilter CreateRegionFilter(IDictionary<string, string> currentTargeting)
        {
            //для работы сайта нам нужно таргетироваться по региону
            //ожидается, что в словаре значений таргетирования к нам придёт запись с ключом "region"
            //и в значении будет список id: id текущего региона и всех родительских через запятую
            if (!currentTargeting.TryGetValue("region", out var regionSrt))
            {
                return new EmptyFilter();
            }

            var regionIds = SplitTargetingRegions(regionSrt)
                .Distinct()
                .ToHashSet();

            if (regionIds.Count == 0)
            {
                return new EmptyFilter();
            }

            return new RegionFilter(regionIds, _logger);
        }

        private static IEnumerable<int> SplitTargetingRegions(string regionsString)
        {
            foreach (var regionPart in regionsString.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(regionPart, out int regionId))
                {
                    yield return regionId;
                }
            }
        }
    }
}