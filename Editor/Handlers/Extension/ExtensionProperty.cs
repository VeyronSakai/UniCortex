using System;

namespace UniCortex.Editor.Handlers.Extension
{
    /// <summary>
    /// Defines a single input parameter for an extension.
    /// </summary>
    public sealed class ExtensionProperty
    {
        public string Name { get; }
        public ExtensionPropertyType Type { get; }
        public string Description { get; }
        public bool Required { get; }

        public ExtensionProperty(string name, ExtensionPropertyType type, string description,
            bool required = false)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Property name must not be null or empty.", nameof(name));

            Name = name;
            Type = type;
            Description = description;
            Required = required;
        }
    }
}
