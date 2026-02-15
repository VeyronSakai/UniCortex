using System.Text;

namespace EditorBridge.Editor.Server
{
    /// <summary>
    /// A lightweight JSON builder that manually constructs JSON strings using StringBuilder.
    /// This avoids a dependency on JsonUtility (which cannot serialize arbitrary objects)
    /// and keeps the output minimal and predictable for REST API responses.
    /// </summary>
    internal static class JsonHelper
    {
        /// <summary>
        /// Builds a JSON object where all values are treated as strings and properly quoted.
        /// Example: Object(("status", "ok")) => {"status":"ok"}
        /// </summary>
        /// <param name="pairs">Key-value pairs; both keys and values are JSON-escaped and quoted.</param>
        /// <returns>A valid JSON object string.</returns>
        public static string Object(params (string key, string value)[] pairs)
        {
            var sb = new StringBuilder();
            sb.Append('{');
            for (var i = 0; i < pairs.Length; i++)
            {
                if (i > 0) sb.Append(',');
                // Write the key as a quoted, escaped JSON string.
                sb.Append('"');
                Escape(sb, pairs[i].key);
                sb.Append("\":");
                // Write the value as a quoted, escaped JSON string.
                sb.Append('"');
                Escape(sb, pairs[i].value);
                sb.Append('"');
            }
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>
        /// Builds a JSON object where values are inserted as raw (pre-formatted) JSON fragments.
        /// This is useful when values are numbers, booleans, arrays, or nested objects that
        /// should NOT be wrapped in quotes.
        /// Example: ObjectRaw(("count", "42")) => {"count":42}
        /// </summary>
        /// <param name="pairs">Key and raw-value pairs; keys are escaped and quoted, values are emitted verbatim.</param>
        /// <returns>A valid JSON object string.</returns>
        public static string ObjectRaw(params (string key, string rawValue)[] pairs)
        {
            var sb = new StringBuilder();
            sb.Append('{');
            for (var i = 0; i < pairs.Length; i++)
            {
                if (i > 0) sb.Append(',');
                // Write the key as a quoted, escaped JSON string.
                sb.Append('"');
                Escape(sb, pairs[i].key);
                sb.Append("\":");
                // Write the raw value directly — caller is responsible for valid JSON.
                sb.Append(pairs[i].rawValue);
            }
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>
        /// Convenience method that produces a standard error response body: {"error":"..."}.
        /// </summary>
        /// <param name="message">A human-readable error description.</param>
        /// <returns>A JSON object with a single "error" key.</returns>
        public static string Error(string message)
        {
            return Object(("error", message));
        }

        /// <summary>
        /// Appends a JSON-escaped version of <paramref name="value"/> to the StringBuilder.
        /// Handles all characters that must be escaped per the JSON specification (RFC 8259):
        /// double-quote, backslash, and control characters (U+0000 through U+001F).
        /// </summary>
        private static void Escape(StringBuilder sb, string value)
        {
            foreach (var c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:
                        // Any remaining control character (U+0000 – U+001F) that doesn't
                        // have a short escape sequence must use the \uXXXX form.
                        if (c < ' ')
                        {
                            sb.Append("\\u");
                            sb.Append(((int)c).ToString("x4"));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
        }
    }
}
