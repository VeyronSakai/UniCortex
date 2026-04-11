using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class InvokeCustomToolRequest
    {
        public string toolName = string.Empty;
        public string argumentsJson = "{}";
    }
}
