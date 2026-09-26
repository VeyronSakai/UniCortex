namespace UniCortex.Editor.Domains.Models
{
    public static class TimelineClipExtrapolationModes
    {
        /// <summary>
        /// Nothing is played outside the clip.
        /// </summary>
        public const string None = "None";

        /// <summary>
        /// Holds the first (pre) or last (post) frame of the clip.
        /// </summary>
        public const string Hold = "Hold";

        /// <summary>
        /// Repeats the clip.
        /// </summary>
        public const string Loop = "Loop";

        /// <summary>
        /// Repeats the clip, alternating forward and backward.
        /// </summary>
        public const string PingPong = "PingPong";

        /// <summary>
        /// Keeps evaluating the clip past its range with its time running on.
        /// </summary>
        public const string Continue = "Continue";
    }
}
