using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.WidgetPlatform.Targeting.Filters;

namespace QA.WidgetPlatform.Targeting.Factories
{
    public class EmptyTargetingFiltersFactory : ITargetingFiltersFactory
    {
        private static readonly ITargetingFilter EmptyFilter = new EmptyFilter();

        public ITargetingFilter StructureFilter(IDictionary<string, string> targeting)
            => EmptyFilter;

        public ITargetingFilter FlattenNodesFilter(IDictionary<string, string> targeting)
            => EmptyFilter;
    }
}
