using System.Collections.Generic;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.WidgetPlatform.Api.TargetingFilters;

namespace QA.WidgetPlatform.Api.Services
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