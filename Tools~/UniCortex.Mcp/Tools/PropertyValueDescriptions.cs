namespace UniCortex.Mcp.Tools;

internal static class PropertyValueDescriptions
{
    internal const string ObjectReference =
        "For object references pass an asset path (e.g. \"Assets/Timelines/Intro.playable\"), " +
        "an asset GUID (32 hex characters), an instanceId of a scene / Prefab object, or \"null\" to clear. " +
        "When the object does not match the field type, a matching component on the GameObject " +
        "(or on the Prefab root) or a matching sub-asset in the same file is assigned instead; " +
        "an error is returned if nothing matches.";

    internal const string Value =
        "The value as a string. Type is auto-detected from the property " +
        "(e.g. \"42\", \"true\", \"(1, 0, 0)\", an enum name). " + ObjectReference;
}
