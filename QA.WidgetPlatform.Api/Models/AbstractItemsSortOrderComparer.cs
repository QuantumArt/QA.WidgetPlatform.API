using QA.DotNetCore.Engine.Abstractions;

namespace QA.WidgetPlatform.Api.Models
{
    public class AbstractItemsSortOrderComparer : IComparer<IAbstractItem>
    {
        public int Compare(IAbstractItem? x, IAbstractItem? y)
        {
            if (x is null)
            {
                return y is null ? 0 : -1;
            }

            if (y is null)
            {
                return 1;
            }

            return x.SortOrder.CompareTo(y.SortOrder);
        }
    }
}
