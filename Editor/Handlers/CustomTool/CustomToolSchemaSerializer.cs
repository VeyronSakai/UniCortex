using System.Text;

namespace UniCortex.Editor.Handlers.CustomTool
{
    internal static class CustomToolSchemaSerializer
    {
        internal static string ToJsonSchema(CustomToolSchema schema)
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

        private static string ToJsonSchemaType(CustomToolPropertyType type)
        {
            switch (type)
            {
                case CustomToolPropertyType.String:
                    return "string";
                case CustomToolPropertyType.Number:
                    return "number";
                case CustomToolPropertyType.Integer:
                    return "integer";
                case CustomToolPropertyType.Boolean:
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
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
