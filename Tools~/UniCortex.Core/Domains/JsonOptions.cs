using System.Text.Json;

namespace UniCortex.Core.Domains;

public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        IncludeFields = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,

        // Unity can return non-finite floats (e.g. infinite tangents of stepped animation keys).
        // JSON has no literal for them, so they are exchanged as the strings "Infinity" / "-Infinity" / "NaN".
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
    };
}
