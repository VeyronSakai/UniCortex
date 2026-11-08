namespace UniCortex.Mcp.Tools;

internal static class PropertyValueDescriptions
{
    internal const string ObjectReference =
        "Object references: an asset path, an asset GUID, an instanceId, or \"null\" to clear. " +
        "If the object does not match the field type, a matching component on it (or on the Prefab root) " +
        "or a matching sub-asset is used; otherwise an error is returned.";

    internal const string Value =
        "Value as a string, parsed by the property type (e.g. \"42\", \"true\", \"(1, 0, 0)\", an enum name). " +
        ObjectReference;
}
