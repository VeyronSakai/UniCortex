namespace UniCortex.Editor.Handlers.CustomTool
{
    /// <summary>
    /// Defines the input schema for a custom MCP tool.
    /// Converted to JSON Schema internally for MCP protocol.
    /// </summary>
    public sealed class CustomToolSchema
    {
        public CustomToolProperty[] Properties { get; }

        public CustomToolSchema(params CustomToolProperty[] properties)
        {
            Properties = properties;
        }
    }
}
