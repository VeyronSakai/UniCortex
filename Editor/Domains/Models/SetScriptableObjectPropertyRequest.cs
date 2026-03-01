using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetScriptableObjectPropertyRequest
    {
        public string assetPath;
        public string propertyPath;
        public string value;
    }
}
