using Microsoft.Extensions.Logging;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.DotNetCore.Engine.QpData;
using QA.WidgetPlatform.Targeting.Filters;

namespace QA.WidgetPlatform.Targeting.Extensions
{
    public static class TargetingExtension
    {
        public static ITargetingFilter AddRelationFilter(
            this IDictionary<string, string> currentTargeting,
            string targetingKey,
            string relationFieldName,
            ILogger logger)
        {
            if (!currentTargeting.TryGetValue(targetingKey, out var targetingValue))
            {
                return new EmptyFilter();
            }

            var regionIds = GetGerionIds(targetingValue);

            if (regionIds.Count == 0)
            {
                return new EmptyFilter();
            }

            return new RelationFilter(item => item.GetRelationIds(relationFieldName), regionIds, logger);
        }

        public static ITargetingFilter AddRelationFilter(
            this IDictionary<string, string> currentTargeting,
            string targetingKey,
            Func<AbstractItem, IEnumerable<int>> relationSelector,
            ILogger logger)
        {
            if (!currentTargeting.TryGetValue(targetingKey, out var targetingValue))
            {
                return new EmptyFilter();
            }

            var regionIds = GetGerionIds(targetingValue);

            if (regionIds.Count == 0)
            {
                return new EmptyFilter();
            }

            return new RelationFilter(relationSelector, regionIds, logger);
        }


        public static ITargetingFilter AddRelationFilter(
            this ITargetingFilter filter,
            IDictionary<string, string> currentTargeting,
            string targetingKey,
            Func<AbstractItem, IEnumerable<int>> relationSelector,
            ILogger logger) =>
            filter.AddFilter(currentTargeting.AddRelationFilter(targetingKey, relationSelector, logger));

        public static ITargetingFilter AddRelationFilter(
            this ITargetingFilter filter,
            IDictionary<string, string> currentTargeting,
            string targetingKey,
            string fieldName,
            ILogger logger) =>
            filter.AddFilter(currentTargeting.AddRelationFilter(targetingKey, fieldName, logger));

        private static HashSet<int> GetGerionIds(string value) =>
            SplitTargetingRegions(value)
                .Distinct()
                .ToHashSet();

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
