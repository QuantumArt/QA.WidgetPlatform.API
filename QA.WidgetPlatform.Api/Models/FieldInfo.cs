namespace QA.WidgetPlatform.Api.Models
{
    public class FieldInfo
    {
        public FieldInfo(string type, object value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; }
        public object Value { get; }
    }
}
