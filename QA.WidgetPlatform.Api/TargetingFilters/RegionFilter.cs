using QA.DotNetCore.Engine.Abstractions;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.DotNetCore.Engine.QpData;

namespace QA.WidgetPlatform.Api.TargetingFilters
{
    public class RegionFilter : BaseTargetingFilter
    {
        private readonly ILogger _logger;
        private readonly ICollection<int> _regionIds;

        public RegionFilter(ICollection<int> regionIds, ILogger logger)
        {
            _regionIds = regionIds;
            _logger = logger;
        }

        public override bool Match(IAbstractItem item)
        {
            if (_regionIds.Count == 0)
            {
                return true;
            }

            if (item is not UniversalAbstractItem uai)
            {
                _logger.LogWarning(
                    "Фильтр не поддерживает AbstractItem данного типа ({ItemType}). Следует использовать " + nameof(UniversalAbstractItemFactory),
                    item.GetType().Name);
                return false;
            }

            return !uai.RegionIds.Any() || uai.RegionIds.Any(_regionIds.Contains);

        }
    }
}