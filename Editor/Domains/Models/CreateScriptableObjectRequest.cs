using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateScriptableObjectRequest
    {
        public string typeName;
        public string assetPath;
    }
}
