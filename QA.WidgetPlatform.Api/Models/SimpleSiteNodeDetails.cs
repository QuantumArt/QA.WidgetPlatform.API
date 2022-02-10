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
    public class SimpleSiteNodeDetails
    {
        public int Id { get; set; }
        public IDictionary<string, FieldInfo> Details { get; set; }

        public SimpleSiteNodeDetails(UniversalAbstractItem item, IEnumerable<string> includeFields = null)
        {
            Id = item.Id;
            var skipIncludeFieldsFilter = includeFields == null || !includeFields.Any();
            
            Details = item.UntypedFields
                .Where(kvp => kvp.Value != null) // думаю, косяк в UniversalAbstractItem, отсекать null-значения скорее всего надо там
                .Where(kvp => skipIncludeFieldsFilter || includeFields.Any(ef => ef.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase)))
                .Where(kvp => skipIncludeFieldsFilter && !Constants.AbstractItemSystemFields.Any(ef => ef.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)))
                .ToDictionary(kvp => kvp.Key, kvp => new FieldInfo
            {
                Value = kvp.Value,
                Type = kvp.Value.GetType().Name // думаю, нужно использовать справочник возможных типов qp, .net типы тут временно
            });
        }
    }
}
