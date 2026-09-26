namespace UniCortex.Editor.Domains.Models
{
    public static class AnimationTangentModes
    {
        /// <summary>
        /// Uses the given inTangent / outTangent as-is. Different values produce a "broken" key with a corner.
        /// Leaving both tangents at 0 flattens the curve at the key (smooth ease in / ease out).
        /// </summary>
        public const string Free = "Free";

        /// <summary>
        /// Computes tangents automatically from the neighbouring keys to make the curve smooth.
        /// The curve may overshoot beyond the key values.
        /// </summary>
        public const string Auto = "Auto";

        /// <summary>
        /// Same as Auto, but clamps the tangents so the curve does not overshoot the key values.
        /// This is the Unity Editor's default mode.
        /// </summary>
        public const string ClampedAuto = "ClampedAuto";

        /// <summary>
        /// Points the tangent straight at the neighbouring key, producing a straight line between keys.
        /// </summary>
        public const string Linear = "Linear";

        /// <summary>
        /// No interpolation: the value is held until the next key and then jumps (step curve).
        /// </summary>
        public const string Constant = "Constant";
    }
}
