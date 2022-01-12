using QA.DotNetCore.Engine.Abstractions;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.DotNetCore.Engine.QpData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.WidgetPlatform.Api
{
    public class MtsRegionFilter : BaseTargetingFilter
    {
        private const string MtsAbstractItemRegionFieldName = "regions";

        public MtsRegionFilter(IEnumerable<int> regionIds)
        {
            RegionIds = regionIds;
        }

        private IEnumerable<int> RegionIds { get; set; }

        public override bool Match(IAbstractItem item)
        {
            if (!RegionIds.Any())
            {
                return true;
            }
            if (!(item is UniversalAbstractItem))
            {
                //так быть не должно, этот фильтр мы должны использоать, когда у нас используется UniversalAbstractItemFactory 
                return false;
            }
            var uai = item as UniversalAbstractItem;

            if (uai.UntypedFields.Keys.Any(k => k.ToLowerInvariant() == MtsAbstractItemRegionFieldName))
            {
                var key = uai.UntypedFields.Keys.First(k => k.ToLowerInvariant() == MtsAbstractItemRegionFieldName);
                IEnumerable<int> regionIds = uai.UntypedFields[key] as IEnumerable<int>;

                if (!regionIds.Any())
                    return true;

                return regionIds.Any(rid => RegionIds.Contains(rid));
            }
            else
            {
                //у всех мтс-ных AbstractItem есть поле "регионы", но даже если нет, то по региону фильтроваться не должен
                return true;
            }
        }
    }
}
