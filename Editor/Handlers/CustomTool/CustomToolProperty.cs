namespace UniCortex.Editor.Handlers.CustomTool
{
    /// <summary>
    /// Defines a single input parameter for a custom MCP tool.
    /// </summary>
    public sealed class CustomToolProperty
    {
        public string Name { get; }
        public CustomToolPropertyType Type { get; }
        public string Description { get; }
        public bool Required { get; }

        public CustomToolProperty(string name, CustomToolPropertyType type, string description,
            bool required = false)
        {
            Name = name;
            Type = type;
            Description = description;
            Required = required;
        }
    }
}
