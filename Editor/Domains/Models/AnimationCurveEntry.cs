using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AnimationCurveEntry
    {
        public string path;
        public string componentType;
        public string assemblyName;
        public string propertyName;
        public List<AnimationCurveKeyEntry> keys;

        public AnimationCurveEntry(string path, string componentType, string assemblyName, string propertyName,
            List<AnimationCurveKeyEntry> keys)
        {
            this.path = path;
            this.componentType = componentType;
            this.assemblyName = assemblyName;
            this.propertyName = propertyName;
            this.keys = keys;
        }
    }
}
