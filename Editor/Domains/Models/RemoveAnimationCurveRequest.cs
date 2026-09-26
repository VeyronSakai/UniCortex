using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RemoveAnimationCurveRequest
    {
        public string assetPath;
        public string animatorRelativePath;
        public string componentType;
        public string assemblyName;
        public string propertyName;
    }
}
