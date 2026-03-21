using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetComponentPropertiesResponse
    {
        public string componentType;
        public List<SerializedPropertyEntry> properties;

        public GetComponentPropertiesResponse(string componentType, List<SerializedPropertyEntry> properties)
        {
            this.componentType = componentType;
            this.properties = properties;
        }
    }
}
