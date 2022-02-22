using System;
using System.Collections.Generic;

namespace QA.WidgetPlatform.Api.Application
{
    public class CaseInSensitiveDictionary<T> : Dictionary<string, T>
    {
        public CaseInSensitiveDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}