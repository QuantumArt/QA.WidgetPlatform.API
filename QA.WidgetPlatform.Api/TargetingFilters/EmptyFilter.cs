using QA.DotNetCore.Engine.Abstractions;
using QA.DotNetCore.Engine.Abstractions.Targeting;

namespace QA.WidgetPlatform.Api.TargetingFilters
{
    public class EmptyFilter : BaseTargetingFilter
    {
        public override bool Match(IAbstractItem item) => true;
    }
}