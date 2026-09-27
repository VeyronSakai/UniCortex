using System.Text.RegularExpressions;

namespace UniCortex.Editor.Handlers.GameObject
{
    // JsonUtility cannot tell a missing field from its default value, so optional fields are detected
    // on the raw JSON body. An explicit null (e.g. "siblingIndex": null) is treated as not specified.
    internal static class JsonFieldPresence
    {
        public static bool HasValue(string json, string fieldName)
        {
            var pattern = "\"" + Regex.Escape(fieldName) + "\"\\s*:(?!\\s*null\\b)";
            return Regex.IsMatch(json, pattern);
        }
    }
}
