using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CustomToolInfo
    {
        public string name;
        public string description;
        public bool readOnly;
        public string inputSchema;
    }
}
