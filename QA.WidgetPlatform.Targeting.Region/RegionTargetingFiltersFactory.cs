using Microsoft.Extensions.Logging;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.DotNetCore.Engine.Targeting.Factories;

namespace QA.WidgetPlatform.Targeting.Region
{
    public class RegionTargetingFiltersFactory : ITargetingFiltersFactory
    {
        private readonly ILogger<RegionTargetingFiltersFactory> _logger;

        public RegionTargetingFiltersFactory(ILogger<RegionTargetingFiltersFactory> logger)
        {
            _logger = logger;
        }

        public ITargetingFilter FlattenNodesFilter(IDictionary<string, string> targeting)
        {
            return CreateRegionFilter(targeting);
        }

        public ITargetingFilter StructureFilter(IDictionary<string, string> targeting)
        {
            return CreateRegionFilter(targeting);
        }

        private ITargetingFilter CreateRegionFilter(IDictionary<string, string> currentTargeting) => currentTargeting
            .AddRelationFilter("region", item => item.RegionIds, _logger);
    }
}
