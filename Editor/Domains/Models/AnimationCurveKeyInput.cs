using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AnimationCurveKeyInput
    {
        public float time;
        public float value;
        public float inTangent;
        public float outTangent;

        // One of AnimationTangentModes. Empty means Free.
        public string tangentMode;
    }
}
