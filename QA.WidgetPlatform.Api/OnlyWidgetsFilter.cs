using QA.DotNetCore.Engine.Abstractions;
using QA.DotNetCore.Engine.Abstractions.Targeting;

namespace QA.WidgetPlatform.Api
{
    public class OnlyWidgetsFilter : BaseTargetingFilter
    {
        public override bool Match(IAbstractItem item)
        {
            return !item.IsPage;
        }
    }
}
