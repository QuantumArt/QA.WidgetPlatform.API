using QA.DotNetCore.Engine.QpData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.WidgetPlatform.Api
{
    /// <summary>
    /// Элемент структуры сайта с информацией по всем полям
    /// </summary>
    public class SiteNodeDetails
    {
        public int Id { get; set; }
        public string Alias { get; set; }
        public string NodeType { get; set; }
        public IDictionary<string, FieldInfo> Details { get; set; }

        protected static List<string> SystemFields = new List<string> 
        { 
            "CONTENT_ITEM_ID", "STATUS_TYPE_ID", "VISIBLE", "ARCHIVE", "CREATED", 
            "MODIFIED", "LAST_MODIFIED_BY", "ITEMID", "PARENT", "DISCRIMINATOR", 
            "EXTENSIONID", "ZONENAME", "ALLOWEDURLPATTERNS", "DENIEDURLPATTERNS",
            "NAME", "INDEXORDER", "VERSIONOF"
        };

        public SiteNodeDetails(UniversalAbstractItem item, IEnumerable<string> excludeFields = null)
        {
            Id = item.Id;
            Alias = item.Alias;
            NodeType = item.Type;

            Details = item.UntypedFields
                .Where(kvp => kvp.Value != null) // думаю, косяк в UniversalAbstractItem, отсекать null-значения скорее всего надо там
                .Where(kvp => !SystemFields.Any(ef => ef.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)))
                .Where(kvp => excludeFields == null || !excludeFields.Any(ef => ef.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)))
                .ToDictionary(kvp => kvp.Key, kvp => new FieldInfo
            {
                Value = kvp.Value,
                Type = kvp.Value.GetType().Name // думаю, нужно использовать справочник возможных типов qp, .net типы тут временно
            });
        }
    }
}
