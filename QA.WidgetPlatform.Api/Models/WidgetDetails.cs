using System.Text.Json.Serialization;
using QA.DotNetCore.Engine.Abstractions;
using QA.DotNetCore.Engine.QpData;

namespace QA.WidgetPlatform.Api.Models
{
    /// <summary>
    /// Виджет
    /// </summary>
    public class WidgetDetails : SiteNodeDetails
    {
        public string Zone { get; set; }
        public string[]? AllowedUrlPatterns { get; set; }
        public string[]? DeniedUrlPatterns { get; set; }
        /// <summary>
        /// Дочерние виджеты, сгруппированные по зонам
        /// </summary>
        public IDictionary<string, WidgetDetails[]>? ChildWidgets { get; set; }

        [JsonIgnore]
        public int SortOrder { get; set; }

        public WidgetDetails(UniversalWidget widget, Func<IAbstractItem, IDictionary<string, WidgetDetails[]>> getChildrenFunc) : base(widget)
        {
            Zone = widget.ZoneName;
            AllowedUrlPatterns = widget.AllowedUrlPatterns;
            DeniedUrlPatterns = widget.DeniedUrlPatterns;
            SortOrder = widget.SortOrder;

            ChildWidgets = getChildrenFunc(widget);

            //небольшой хак, чтобы сериализованный объект был меньше
            if (!ChildWidgets.Any())
                ChildWidgets = null;
            if (!AllowedUrlPatterns.Any())
                AllowedUrlPatterns = null;
            if (!DeniedUrlPatterns.Any())
                DeniedUrlPatterns = null;
        }
    }
}
