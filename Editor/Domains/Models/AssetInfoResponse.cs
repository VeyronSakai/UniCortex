using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AssetInfoResponse
    {
        public string assetPath;
        public string type;
        public List<SerializedPropertyEntry> properties;

        public AssetInfoResponse(string assetPath, string type, List<SerializedPropertyEntry> properties)
        {
            this.assetPath = assetPath;
            this.type = type;
            this.properties = properties;
        }
    }
}
