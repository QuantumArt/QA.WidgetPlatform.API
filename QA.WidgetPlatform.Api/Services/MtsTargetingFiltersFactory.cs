#nullable enable
using System.Collections.Generic;
using System.Linq;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.WidgetPlatform.Api.Application;
using QA.WidgetPlatform.Api.TargetingFilters;

namespace QA.WidgetPlatform.Api.Services
{
    public class MtsTargetingFiltersFactory : ITargetingFiltersFactory
    {
        public ITargetingFilter? StructureFilter(IDictionary<string, string> targeting)
        {
            return CreateRegionFilter(targeting);
        }

        public ITargetingFilter? FlattenNodesFilter(IDictionary<string, string> targeting)
        {
            ITargetingFilter?[] filters =
            {
                CreateRegionFilter(targeting),
                CreateNodeTypesFilter(targeting)
            };
            return filters.Combine();
        }

        private static MtsRegionFilter? CreateRegionFilter(IDictionary<string, string> currentTargeting)
        {
            //для работы мтс-ного сайта нам нужно таргетироваться по региону
            //ожидается, что в словаре значений таргетирования к нам придёт запись с ключом "region"
            //и в значении будет список id: id текущего региона и всех родительских через запятую
            if (!currentTargeting.TryGetValue("region", out var regionSrt))
                return null;

            var idList = regionSrt
                .Split(',')
                .Select(t => int.TryParse(t, out int parsed) ? parsed : 0)
                .Where(id => id > 0)
                .ToArray();

            return new MtsRegionFilter(idList);
        }

        private static NodeTypesFilter? CreateNodeTypesFilter(IDictionary<string, string> currentTargeting)
        {
            if (!currentTargeting.TryGetValue("nodeType", out var nodeTypesStr))
                return null;

            var nodeTypes = nodeTypesStr
                .Split(Constants.ArraySeparator)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();
            return new NodeTypesFilter(nodeTypes);
        }
    }
}