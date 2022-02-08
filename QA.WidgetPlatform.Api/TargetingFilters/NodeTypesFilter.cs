using System;
using System.Linq;
using QA.DotNetCore.Engine.Abstractions;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.DotNetCore.Engine.QpData;

namespace QA.WidgetPlatform.Api.TargetingFilters
{
    public class NodeTypesFilter : BaseTargetingFilter
    {
        private readonly string[] _nodeTypes;

        public NodeTypesFilter(string[] nodeTypes)
        {
            _nodeTypes = nodeTypes;
        }


        public override bool Match(IAbstractItem item)
        {
            if (_nodeTypes == null || !_nodeTypes.Any())
                return true;

            if (!(item is UniversalAbstractItem uai))
                return false;

            return _nodeTypes.Contains(uai.Type, StringComparer.OrdinalIgnoreCase);
        }
    }
}
