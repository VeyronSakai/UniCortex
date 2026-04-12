#nullable enable

using System.Text;

namespace UniCortex.Editor.Handlers.Extension
{
    internal static class ExtensionSchemaSerializer
    {
        internal static string? ToJsonSchema(ExtensionSchema? schema)
        {
            if (schema == null || schema.Properties == null || schema.Properties.Length == 0)
            {
                return null;
            }

            var sb = new StringBuilder();
            sb.Append("{\"type\":\"object\",\"properties\":{");

            for (var i = 0; i < schema.Properties.Length; i++)
            {
                if (i > 0) sb.Append(',');
                var prop = schema.Properties[i];
                sb.Append('"');
                sb.Append(EscapeJsonString(prop.Name));
                sb.Append("\":{\"type\":\"");
                sb.Append(ToJsonSchemaType(prop.Type));
                sb.Append('"');
                if (!string.IsNullOrEmpty(prop.Description))
                {
                    sb.Append(",\"description\":\"");
                    sb.Append(EscapeJsonString(prop.Description));
                    sb.Append('"');
                }

                sb.Append('}');
            }

            sb.Append('}');

            var hasRequired = false;
            for (var i = 0; i < schema.Properties.Length; i++)
            {
                if (!schema.Properties[i].Required) continue;

                if (!hasRequired)
                {
                    sb.Append(",\"required\":[");
                    hasRequired = true;
                }
                else
                {
                    sb.Append(',');
                }

                sb.Append('"');
                sb.Append(EscapeJsonString(schema.Properties[i].Name));
                sb.Append('"');
            }

            if (hasRequired)
            {
                sb.Append(']');
            }

            sb.Append('}');
            return sb.ToString();
        }

        private static string ToJsonSchemaType(ExtensionPropertyType type)
        {
            switch (type)
            {
                case ExtensionPropertyType.String:
                    return "string";
                case ExtensionPropertyType.Number:
                    return "number";
                case ExtensionPropertyType.Integer:
                    return "integer";
                case ExtensionPropertyType.Boolean:
                    return "boolean";
                default:
                    return "string";
            }
        }

        private static string EscapeJsonString(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            var sb = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                switch (c)
                {
                    case '"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        if (c < ' ')
                        {
                            sb.AppendFormat("\\u{0:x4}", (int)c);
                        }
                        else
                        {
                            sb.Append(c);
                        }

                        break;
                }
            }

            return sb.ToString();
        }
    }
}
