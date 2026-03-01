using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ScriptableObjectInfoResponse
    {
        public string assetPath;
        public string type;
        public List<SerializedPropertyEntry> properties;

        public ScriptableObjectInfoResponse(string assetPath, string type, List<SerializedPropertyEntry> properties)
        {
            this.assetPath = assetPath;
            this.type = type;
            this.properties = properties;
        }
    }
}
