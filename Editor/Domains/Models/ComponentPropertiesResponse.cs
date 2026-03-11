using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ComponentPropertiesResponse
    {
        public string componentType;
        public List<SerializedPropertyEntry> properties;

        public ComponentPropertiesResponse(string componentType, List<SerializedPropertyEntry> properties)
        {
            this.componentType = componentType;
            this.properties = properties;
        }
    }
}
