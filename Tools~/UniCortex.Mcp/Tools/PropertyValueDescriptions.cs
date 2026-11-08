namespace UniCortex.Mcp.Tools;

internal static class PropertyValueDescriptions
{
    internal const string ObjectReference =
        "Object references: an asset path, an asset GUID, an instanceId, or \"null\" to clear.";

    internal const string Value =
        "Value as a string, parsed by the property type (e.g. \"42\", \"true\", \"(1, 0, 0)\", an enum name). " +
        ObjectReference;
}
