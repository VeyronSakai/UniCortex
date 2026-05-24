namespace UniCortex.Editor.Domains.Models
{
    // Names of HTTP query parameters. Each value must match the corresponding
    // request DTO field name, because the Core client builds the query string
    // from those field names (see UnityEditorClient.BuildQueryString).
    public static class QueryParameterNames
    {
        public const string Verbose = "verbose";
        public const string Category = "category";
    }
}
