using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetAssetPropertyRequest
    {
        public string assetPath;
        public string propertyPath;
        public string value;
    }
}
