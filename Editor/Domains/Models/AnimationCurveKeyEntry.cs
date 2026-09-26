using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AnimationCurveKeyEntry
    {
        public float time;
        public float value;
        public float inTangent;
        public float outTangent;
        public string leftTangentMode;
        public string rightTangentMode;

        public AnimationCurveKeyEntry(float time, float value, float inTangent, float outTangent,
            string leftTangentMode, string rightTangentMode)
        {
            this.time = time;
            this.value = value;
            this.inTangent = inTangent;
            this.outTangent = outTangent;
            this.leftTangentMode = leftTangentMode;
            this.rightTangentMode = rightTangentMode;
        }
    }
}
