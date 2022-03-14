using System;
using System.Collections.Generic;
using System.Linq;
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

            Details = item.UntypedFields
                .Where(kvp =>
                    kvp.Value !=
                    null) // думаю, косяк в UniversalAbstractItem, отсекать null-значения скорее всего надо там
                .Where(kvp =>
                    !Constants.AbstractItemSystemFields.Any(ef =>
                        ef.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)))
                .Where(kvp =>
                    excludeFields == null || !excludeFields.Any(ef =>
                        ef.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)))
                .ToDictionary(kvp => kvp.Key, kvp =>
                    new FieldInfo(
                        // думаю, нужно использовать справочник возможных типов qp, .net типы тут временно
                        kvp.Value.GetType().Name,
                        kvp.Value
                    ));
        }
    }
}