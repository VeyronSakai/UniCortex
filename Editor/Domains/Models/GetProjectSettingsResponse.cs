using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetProjectSettingsResponse
    {
        public string category;
        public List<SerializedPropertyEntry> properties;

        public GetProjectSettingsResponse(string category, List<SerializedPropertyEntry> properties)
        {
            this.category = category;
            this.properties = properties;
        }
    }
}
