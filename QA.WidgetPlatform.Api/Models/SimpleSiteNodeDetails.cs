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
        public int Id { get; }
        public IDictionary<string, FieldInfo> Details { get; }

        public SimpleSiteNodeDetails(UniversalAbstractItem item, IEnumerable<string> includeFields = null)
        {
            Id = item.Id;

            var detailsDto = item.UntypedFields
                .Where(kvp =>
                    kvp.Value !=
                    null); // думаю, косяк в UniversalAbstractItem, отсекать null-значения скорее всего надо там 

            if (includeFields == null || !includeFields.Any())
            {
                detailsDto = detailsDto
                    .Where(kvp =>
                        !Constants.AbstractItemSystemFields.Any(ef =>
                            ef.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)));
            }
            else
            {
                detailsDto = detailsDto
                    .Where(kvp => includeFields.Any(ef => ef.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase)));
            }

            Details = detailsDto.ToDictionary(kvp => kvp.Key, kvp => new FieldInfo
            {
                Value = kvp.Value,
                Type = kvp.Value.GetType()
                    .Name // думаю, нужно использовать справочник возможных типов qp, .net типы тут временно
            });
        }
    }
}