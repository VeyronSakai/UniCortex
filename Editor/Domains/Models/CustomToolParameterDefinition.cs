#nullable enable

using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CustomToolParameterDefinition
    {
        public string name;
        public string type;
        public bool required;
        public string description;
        public string itemType;

        public CustomToolParameterDefinition(
            string name,
            string type,
            bool required,
            string description = "",
            string itemType = "")
        {
            this.name = name;
            this.type = type;
            this.required = required;
            this.description = description;
            this.itemType = itemType;
        }
    }
}
