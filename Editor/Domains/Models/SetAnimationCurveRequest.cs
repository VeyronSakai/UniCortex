using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetAnimationCurveRequest
    {
        public string assetPath;
        public string animatorRelativePath;
        public string componentType;
        public string assemblyName;
        public string propertyName;
        public List<AnimationCurveKeyInput> keys;
    }
}
