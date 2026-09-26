using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UniCortex.Core.Domains;

namespace UniCortex.Core.Infrastructures;

/// <summary>
/// Bridges JSON between the Core (System.Text.Json) and the Unity Editor (JsonUtility) for non-finite
/// floating-point values. JsonUtility writes and reads infinity / NaN as bare tokens
/// (e.g. <c>"inTangent":Infinity</c>), which are not valid JSON. System.Text.Json only accepts them as
/// quoted named literals (<c>"Infinity"</c>) when <see cref="JsonNumberHandling.AllowNamedFloatingPointLiterals"/>
/// is enabled, as it is in <see cref="JsonOptions.Default"/>.
/// </summary>
internal static class UnityJson
{
    private static readonly string[] s_nonFiniteLiterals = ["-Infinity", "Infinity", "NaN"];

    /// <summary>
    /// Options for serializing requests sent to Unity: non-finite floats are written as bare tokens so
    /// that JsonUtility can read them.
    /// </summary>
    public static readonly JsonSerializerOptions RequestOptions = CreateRequestOptions();

    /// <summary>
    /// Deserializes a response body produced by JsonUtility.
    /// </summary>
    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(QuoteNonFiniteLiterals(json), JsonOptions.Default);
    }

    /// <summary>
    /// Wraps bare Infinity / -Infinity / NaN tokens in quotes. Tokens inside string values are left
    /// untouched (e.g. a GameObject named "Infinity").
    /// </summary>
    public static string QuoteNonFiniteLiterals(string json)
    {
        if (!json.Contains("Infinity", StringComparison.Ordinal) && !json.Contains("NaN", StringComparison.Ordinal))
        {
            return json;
        }

        var builder = new StringBuilder(json.Length + 16);
        var inString = false;
        var escaped = false;
        for (var i = 0; i < json.Length; i++)
        {
            var c = json[i];
            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (c == '\\')
                {
                    escaped = true;
                }
                else if (c == '"')
                {
                    inString = false;
                }

                builder.Append(c);
                continue;
            }

            if (c == '"')
            {
                inString = true;
                builder.Append(c);
                continue;
            }

            var literal = FindLiteralAt(json, i);
            if (literal != null)
            {
                builder.Append('"').Append(literal).Append('"');
                i += literal.Length - 1;
                continue;
            }

            builder.Append(c);
        }

        return builder.ToString();
    }

    private static string? FindLiteralAt(string json, int index)
    {
        foreach (var literal in s_nonFiniteLiterals)
        {
            if (string.CompareOrdinal(json, index, literal, 0, literal.Length) == 0)
            {
                return literal;
            }
        }

        return null;
    }

    private static JsonSerializerOptions CreateRequestOptions()
    {
        var options = new JsonSerializerOptions(JsonOptions.Default);
        options.Converters.Add(new BareNonFiniteSingleConverter());
        options.Converters.Add(new BareNonFiniteDoubleConverter());
        return options;
    }

    private sealed class BareNonFiniteSingleConverter : JsonConverter<float>
    {
        public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetSingle();
        }

        public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
        {
            if (float.IsFinite(value))
            {
                writer.WriteNumberValue(value);
                return;
            }

            // Bare token for JsonUtility; invalid JSON by design, so skip validation.
            writer.WriteRawValue(ToLiteral(value), skipInputValidation: true);
        }
    }

    private sealed class BareNonFiniteDoubleConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetDouble();
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            if (double.IsFinite(value))
            {
                writer.WriteNumberValue(value);
                return;
            }

            writer.WriteRawValue(ToLiteral(value), skipInputValidation: true);
        }
    }

    private static string ToLiteral(double value)
    {
        if (double.IsNaN(value))
        {
            return "NaN";
        }

        return double.IsPositiveInfinity(value) ? "Infinity" : "-Infinity";
    }
}
