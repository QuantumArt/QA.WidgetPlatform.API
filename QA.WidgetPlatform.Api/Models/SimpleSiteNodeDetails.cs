using QA.DotNetCore.Engine.QpData;
using QA.WidgetPlatform.Api.Application;

namespace QA.WidgetPlatform.Api.Models
{
    /// <summary>
    /// Элемент структуры сайта с информацией по всем полям
    /// </summary>
    public class SimpleSiteNodeDetails
    {
        public int Id { get; }
        public IDictionary<string, FieldInfo> Details { get; }

        public SimpleSiteNodeDetails(UniversalAbstractItem item, IEnumerable<string>? includeFields = null)
        {
            Id = item.Id;
            Details = new Dictionary<string, FieldInfo>(item.UntypedFields.Count);

            var filteredDetailsFields = (includeFields is null || !includeFields.Any())
                ? item.UntypedFields.ExceptSystemNames()
                : item.UntypedFields.FilterByFieldNames(
                    new HashSet<string>(includeFields, StringComparer.OrdinalIgnoreCase));

            foreach ((string fieldName, object fieldValue) in filteredDetailsFields)
            {
                Details.Add(fieldName, new FieldInfo(fieldValue));
            }
        }
    }
}
