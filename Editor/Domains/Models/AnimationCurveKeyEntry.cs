using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AnimationCurveKeyEntry
    {
        public float time;
        public float value;

        // Slope (value change per second) of the curve at this key.
        // inTangent is the incoming (left) side, outTangent is the outgoing (right) side.
        // Different values produce a "broken" key with a corner.
        public float inTangent;
        public float outTangent;

        // How each tangent is determined (AnimationUtility.TangentMode name: Free, Auto, ClampedAuto,
        // Linear, Constant). Left corresponds to inTangent and right to outTangent; the in/out vs
        // left/right naming mirrors Unity's Keyframe and AnimationUtility APIs respectively.
        // For modes other than Free, inTangent/outTangent hold the values computed by Unity.
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
