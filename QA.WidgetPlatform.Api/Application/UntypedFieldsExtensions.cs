namespace QA.WidgetPlatform.Api.Application
{
    public static class UntypedFieldsExtensions
    {
        public static IEnumerable<KeyValuePair<string, object>> ExceptFieldNames(
            this IEnumerable<KeyValuePair<string, object>> fieldsCollection,
            IEnumerable<string>? excludedFieldNames)
        {
            return (excludedFieldNames is null)
                ? fieldsCollection
                : fieldsCollection.ExceptFieldNamesCore(new HashSet<string>(excludedFieldNames, StringComparer.OrdinalIgnoreCase));
        }

        private static IEnumerable<KeyValuePair<string, object>> ExceptFieldNamesCore(
            this IEnumerable<KeyValuePair<string, object>> fieldsCollection,
            ICollection<string> excludedFieldNames)
        {
            foreach (var pair in fieldsCollection)
            {
                string fieldName = pair.Key;

                if (!excludedFieldNames.Contains(fieldName))
                {
                    yield return pair;
                }
            }
        }

        public static IEnumerable<KeyValuePair<string, object>> ExceptSystemNames(
            this IEnumerable<KeyValuePair<string, object>> fieldsCollection)
        {
            return fieldsCollection.Where(IsNotSystemName);
        }

        private static bool IsNotSystemName(KeyValuePair<string, object> fieldNameValuePair)
        {
            var fieldName = fieldNameValuePair.Key;

            return !Constants.AbstractItemSystemFields.Contains(fieldName);
        }

        public static IEnumerable<KeyValuePair<string, object>> FilterByFieldNames(
            this IEnumerable<KeyValuePair<string, object>> fieldsCollection,
            ICollection<string> includedNames)
        {
            foreach (var pair in fieldsCollection)
            {
                string fieldName = pair.Key;

                if (includedNames.Contains(fieldName))
                {
                    yield return pair;
                }
            }
        }
    }
}
