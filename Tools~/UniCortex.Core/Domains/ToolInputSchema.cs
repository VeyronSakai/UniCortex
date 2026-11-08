using System.Text.Json;
using System.Text.Json.Nodes;

namespace UniCortex.Core.Domains;

/// <summary>
/// Shrinks the input schemas of tools, which AI agents read as tokens.
/// </summary>
public static class ToolInputSchema
{
    private const string DefaultKey = "default";
    private const string TypeKey = "type";
    private const string NullType = "null";

    /// <summary>
    /// Removes <c>"default": null</c> and the <c>"null"</c> type from nullable types such as
    /// <c>["integer", "null"]</c>. An omitted optional parameter is already null, so they add no information.
    /// </summary>
    public static JsonElement Simplify(JsonElement schema)
    {
        var node = JsonNode.Parse(schema.GetRawText());
        SimplifyNode(node);
        using var document = JsonDocument.Parse(node?.ToJsonString() ?? "null");
        return document.RootElement.Clone();
    }

    private static void SimplifyNode(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject jsonObject:
                SimplifyObject(jsonObject);
                break;
            case JsonArray jsonArray:
                foreach (var item in jsonArray)
                {
                    SimplifyNode(item);
                }

                break;
        }
    }

    private static void SimplifyObject(JsonObject jsonObject)
    {
        if (jsonObject.TryGetPropertyValue(DefaultKey, out var defaultValue) && defaultValue is null)
        {
            jsonObject.Remove(DefaultKey);
        }

        if (jsonObject[TypeKey] is JsonArray types && TryGetNonNullType(types, out var nonNullType))
        {
            jsonObject[TypeKey] = nonNullType;
        }

        foreach (var value in jsonObject.Select(property => property.Value).ToList())
        {
            SimplifyNode(value);
        }
    }

    private static bool TryGetNonNullType(JsonArray types, out string nonNullType)
    {
        nonNullType = string.Empty;
        if (types.Count != 2)
        {
            return false;
        }

        var names = types.Select(type => type?.GetValueKind() == JsonValueKind.String ? type.GetValue<string>() : null)
            .ToList();
        if (!names.Contains(NullType))
        {
            return false;
        }

        var other = names.FirstOrDefault(name => name != NullType);
        if (other is null)
        {
            return false;
        }

        nonNullType = other;
        return true;
    }
}
