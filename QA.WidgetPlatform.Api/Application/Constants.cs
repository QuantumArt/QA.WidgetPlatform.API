﻿namespace QA.WidgetPlatform.Api.Application
{
    public static class Constants
    {
        public static readonly ICollection<string> AbstractItemSystemFields =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CONTENT_ITEM_ID",
                "STATUS_TYPE_ID",
                "VISIBLE",
                "ARCHIVE",
                "CREATED",
                "MODIFIED",
                "LAST_MODIFIED_BY",
                "ITEMID",
                "PARENT",
                "DISCRIMINATOR",
                "EXTENSIONID",
                "ZONENAME",
                "ALLOWEDURLPATTERNS",
                "DENIEDURLPATTERNS",
                "NAME",
                "INDEXORDER",
                "VERSIONOF"
            };

        public const char ArraySeparator = ',';
    }
}
