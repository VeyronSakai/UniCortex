using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateScriptableObjectRequest
    {
        public string type;
        public string assetPath;
    }
}
