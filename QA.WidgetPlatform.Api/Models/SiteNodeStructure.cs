using System;
using System.Collections.Generic;
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
        public SiteNode(UniversalAbstractItem abstractItem, ITargetingFilter targetingFlt, int? deep = null, IEnumerable<string> includeFields = null)
        {
            Id = abstractItem.Id;
            Alias = abstractItem.Alias;
            NodeType = abstractItem.Type;

            if (IsDeepAvailable(deep--))
            {
                var children = abstractItem.GetChildren<UniversalAbstractItem>(targetingFlt);
                if (children.Any())
                {
                    Children = children
                        .OrderBy(ai => ai.SortOrder)
                        .Select(ai => new SiteNode(ai, targetingFlt, deep, includeFields))
                        .ToArray();
                }
            }

            if (includeFields != null && includeFields.Any())
            {
                Details = abstractItem.UntypedFields.Where(kvp => kvp.Value != null) // думаю, косяк в UniversalAbstractItem, отсекать null-значения скорее всего надо там
                    .Where(kvp => includeFields.Any(ef => ef.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase)))
                    .ToDictionary(kvp => kvp.Key, kvp => new FieldInfo
                    {
                        Value = kvp.Value,
                        Type = kvp.Value.GetType()
                            .Name // думаю, нужно использовать справочник возможных типов qp, .net типы тут временно
                    });
            }

            static bool IsDeepAvailable(int? deep)
            {
                return !deep.HasValue || deep.Value > 0;
            }
        }

        public int Id { get; }
        public string Alias { get; }
        public string NodeType { get; }
        public SiteNode[] Children { get; }
        public IDictionary<string, FieldInfo> Details { get; }
    }
}
