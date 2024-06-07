namespace QA.WidgetPlatform.Api.Models
{
    public class FieldsSettings
    {
        public bool IncludeNullFieldsInNode { get; set; }

        public FieldsSettings()
        {
            IncludeNullFieldsInNode = false;
        }
    }
}
