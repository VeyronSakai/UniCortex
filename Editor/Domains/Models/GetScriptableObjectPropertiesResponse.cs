using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetScriptableObjectPropertiesResponse
    {
        public string typeName;
        public List<SerializedPropertyEntry> properties;

        public GetScriptableObjectPropertiesResponse(string typeName, List<SerializedPropertyEntry> properties)
        {
            this.typeName = typeName;
            this.properties = properties;
        }
    }
}
