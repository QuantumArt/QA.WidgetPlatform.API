using QA.DotNetCore.Engine.Abstractions;
using QA.DotNetCore.Engine.QpData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QA.WidgetPlatform.Api
{
    /// <summary>
    /// Виджет
    /// </summary>
    public class WidgetDetails : SiteNodeDetails
    {
        public string Zone { get; set; }
        public string[] AllowedUrlPatterns { get; set; }
        public string[] DeniedUrlPatterns { get; set; }
        /// <summary>
        /// Дочерние виджеты, сгруппированные по зонам
        /// </summary>
        public IDictionary<string, WidgetDetails[]> ChildWidgets { get; set; }

        [JsonIgnore]
        public int SortOrder { get; set; }

        public WidgetDetails(UniversalAbstractItem item, Func<IAbstractItem, IDictionary<string, WidgetDetails[]>> getChildrenFunc) : base(item)
        {
            Zone = item.UntypedFields["ZONENAME"].ToString();
            if (item.UntypedFields.ContainsKey("ALLOWEDURLPATTERNS") && item.UntypedFields["ALLOWEDURLPATTERNS"] != null)
            {
                AllowedUrlPatterns = item.UntypedFields["ALLOWEDURLPATTERNS"].ToString().Split(new char[] { '\n', '\r', ';', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            if (item.UntypedFields.ContainsKey("DENIEDURLPATTERNS") && item.UntypedFields["DENIEDURLPATTERNS"] != null)
            {
                DeniedUrlPatterns = item.UntypedFields["DENIEDURLPATTERNS"].ToString().Split(new char[] { '\n', '\r', ';', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            }

            ChildWidgets = getChildrenFunc(item);
            if (!ChildWidgets.Any())
                ChildWidgets = null;//чтобы сериализованный объект был меньше

            SortOrder = item.SortOrder;
        }
    }
}
