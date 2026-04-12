namespace UniCortex.Editor.Handlers.Extension
{
    /// <summary>
    /// Defines the input schema for an extension.
    /// Converted to JSON Schema internally for MCP protocol.
    /// </summary>
    public sealed class ExtensionSchema
    {
        public ExtensionProperty[] Properties { get; }

        public ExtensionSchema(params ExtensionProperty[] properties)
        {
            Properties = properties;
        }
    }
}
