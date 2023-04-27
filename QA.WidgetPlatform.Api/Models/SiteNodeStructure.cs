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
            int? deep = null, bool isDefinitionFields = false)
        {
            Id = abstractItem.Id;
            Alias = abstractItem.Alias;
            NodeType = abstractItem.Type;

            if (isDefinitionFields)
            {
                FrontModuleUrl = abstractItem.Definition?.FrontModuleUrl;
                FrontModuleName = abstractItem.Definition?.FrontModuleName;
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
                        Children[i] = new SiteNode(child, targetingFlt, includeFields, deep, isDefinitionFields);
                    }
                }
            }

            if (includeFields.Count > 0)
            {
                Details = new Dictionary<string, FieldInfo>(abstractItem.UntypedFields.Count);

                var filteredDetailsFields = abstractItem.UntypedFields
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
