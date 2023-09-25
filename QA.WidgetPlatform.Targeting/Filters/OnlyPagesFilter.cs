using QA.DotNetCore.Engine.Abstractions;
using QA.DotNetCore.Engine.Abstractions.Targeting;

namespace QA.WidgetPlatform.Targeting.Filters
{
    public class OnlyPagesFilter : BaseTargetingFilter
    {
        public override bool Match(IAbstractItem item)
        {
            return item.IsPage;
        }
    }
}
