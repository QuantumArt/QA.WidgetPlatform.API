using System.Buffers;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.DotNetCore.Engine.QpData;
using QA.WidgetPlatform.Api.Application;

namespace QA.WidgetPlatform.Api.Models
{

    /// <summary>
    /// Элемент структуры сайта
    /// </summary>
    public class SiteNode
    {
        public int Id { get; }
        public string Alias { get; }
        public string NodeType { get; }
        public string? FrontModuleUrl { get; }
        public string? FrontModuleName { get; }
        public SiteNode[]? Children { get; }
        public IDictionary<string, FieldInfo>? Details { get; }

        public SiteNode(
            UniversalAbstractItem abstractItem,
            ITargetingFilter targetingFlt,
            ICollection<string> includeFields,
            int? deep = null, bool fillDefinitionDetails = false)
        {
            Id = abstractItem.Id;
            Alias = abstractItem.Alias;
            NodeType = abstractItem.Type;

            if (fillDefinitionDetails)
            {
                FrontModuleUrl = abstractItem.DefinitionDetails?.FrontModuleUrl;
                FrontModuleName = abstractItem.DefinitionDetails?.FrontModuleName;
            }

            if (IsDeepAvailable(deep--))
            {
                var abstractItemChildren = abstractItem
                    .GetChildren<UniversalAbstractItem>(targetingFlt)
                    .ToArray();

                if (abstractItemChildren.Length > 0)
                {
                    Array.Sort(abstractItemChildren, new AbstractItemsSortOrderComparer());

                    Children = new SiteNode[abstractItemChildren.Length];

                    for (int i = 0; i < abstractItemChildren.Length; i++)
                    {
                        var child = abstractItemChildren[i];
                        Children[i] = new SiteNode(child, targetingFlt, includeFields, deep, fillDefinitionDetails);
                    }
                }
            }

            if (includeFields.Count > 0)
            {
                var untypedFields = abstractItem.GetUntypedFields();
                Details = new Dictionary<string, FieldInfo>(untypedFields.Count);

                var filteredDetailsFields = untypedFields
                    .FilterByFieldNames(includeFields);

                foreach ((string fieldName, object fieldValue) in filteredDetailsFields)
                {
                    Details.Add(fieldName, new FieldInfo(fieldValue));
                }
            }

            static bool IsDeepAvailable(int? deep)
            {
                return !deep.HasValue || deep.Value > 0;
            }
        }
    }
}
