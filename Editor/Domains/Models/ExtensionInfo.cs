#nullable enable

using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ExtensionInfo
    {
        public string name = "";
        public string description = "";
        public bool readOnly;
        public string? inputSchema;
    }
}
