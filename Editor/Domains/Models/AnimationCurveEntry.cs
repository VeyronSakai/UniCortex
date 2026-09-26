using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AnimationCurveEntry
    {
        public string animatorRelativePath;
        public string componentType;
        public string assemblyName;
        public string propertyName;
        public List<AnimationCurveKeyEntry> keys;

        public AnimationCurveEntry(string animatorRelativePath, string componentType, string assemblyName,
            string propertyName, List<AnimationCurveKeyEntry> keys)
        {
            this.animatorRelativePath = animatorRelativePath;
            this.componentType = componentType;
            this.assemblyName = assemblyName;
            this.propertyName = propertyName;
            this.keys = keys;
        }
    }
}
