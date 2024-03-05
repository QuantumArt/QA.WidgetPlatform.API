using QA.DotNetCore.Engine.Abstractions.OnScreen;
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
        public bool Published { get; set; }
        public IDictionary<string, FieldInfo> Details { get; set; }

        public SiteNodeDetails(UniversalAbstractItem item, IEnumerable<string>? includeFields = null)
        {
            Id = item.Id;
            Alias = item.Alias;
            NodeType = item.Type;
            Published = (bool)item.GetMetadata(OnScreenWidgetMetadataKeys.Published);
            var untypedFields = item.GetUntypedFields();
            Details = new Dictionary<string, FieldInfo>(untypedFields.Count);

            var filteredDetailsFields = (includeFields is null || !includeFields.Any())
                ? untypedFields.ExceptSystemNames()
                : untypedFields.FilterByFieldNames(
                    new HashSet<string>(includeFields, StringComparer.OrdinalIgnoreCase));
            
            foreach ((string fieldName, object fieldValue) in filteredDetailsFields)
            {
                Details.Add(fieldName, new FieldInfo(fieldValue));
            }
        }
    }
}
