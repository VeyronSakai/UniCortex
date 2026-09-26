namespace UniCortex.Editor.Domains.Models
{
    public static class AnimationTangentModes
    {
        // Uses inTangent / outTangent as given.
        public const string Free = "Free";
        public const string Auto = "Auto";
        public const string ClampedAuto = "ClampedAuto";
        public const string Linear = "Linear";
        public const string Constant = "Constant";

        // Free with both tangents flattened to 0 (smooth ease in / ease out).
        public const string EaseInOut = "EaseInOut";
    }
}
