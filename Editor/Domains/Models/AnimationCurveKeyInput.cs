using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AnimationCurveKeyInput
    {
        public float time;
        public float value;
        // Slope (value change per second) on the incoming (left) and outgoing (right) side of the key.
        // Only used when tangentMode is Free; other modes compute the tangents automatically.
        public float inTangent;
        public float outTangent;

        // One of AnimationTangentModes. Empty means Free.
        // Applied to both sides of the key (left = in side, right = out side).
        public string tangentMode;
    }
}
