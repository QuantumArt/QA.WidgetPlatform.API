using QA.DotNetCore.Engine.QpData;
using QA.WidgetPlatform.Api.Application;

namespace QA.WidgetPlatform.Api.Models
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

        public SiteNodeDetails(UniversalAbstractItem item, IEnumerable<string>? excludeFields = null)
        {
            Id = item.Id;
            Alias = item.Alias;
            NodeType = item.Type;

            Details = new Dictionary<string, FieldInfo>(item.UntypedFields.Count);

            var filteredDetailsFields = item.UntypedFields
                .ExceptSystemNames()
                .ExceptFieldNames(excludeFields);

            foreach ((string fieldName, object fieldValue) in filteredDetailsFields)
            {
                Details.Add(fieldName, new FieldInfo(fieldValue));
            }
        }
    }
}
