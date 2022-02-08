using System.Linq;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.DotNetCore.Engine.QpData;

namespace QA.WidgetPlatform.Api.Models
{
    /// <summary>
    /// Элемент структуры сайта
    /// </summary>
    public class SiteNode
    {
        public SiteNode(UniversalAbstractItem abstractItem, ITargetingFilter targetingFlt)
        {
            Id = abstractItem.Id;
            Alias = abstractItem.Alias;
            NodeType = abstractItem.Type;

            var children = abstractItem.GetChildren<UniversalAbstractItem>(targetingFlt);
            if (children.Any())
            {
                Children = children
                    .OrderBy(ai => ai.SortOrder)
                    .Select(ai => new SiteNode(ai, targetingFlt))
                    .ToArray();
            }
        }

        public int Id { get; set; }
        public string Alias { get; set; }
        public string NodeType { get; set; }
        public SiteNode[] Children { get; set; }
    }
}
