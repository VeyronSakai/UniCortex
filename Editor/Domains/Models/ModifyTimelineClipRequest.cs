using System;

namespace UniCortex.Editor.Domains.Models
{
    /// <summary>
    /// Only the fields that are set (non-null) are applied to the clip.
    /// </summary>
    [Serializable]
    public class ModifyTimelineClipRequest
    {
        public int instanceId;
        public int trackIndex;
        public int clipIndex;
        public string? displayName;
        public double? start;
        public double? duration;
        public double? timeScale;
        public double? clipIn;
        public double? easeInDuration;
        public double? easeOutDuration;

        /// <summary>
        /// One of <see cref="TimelineClipExtrapolationModes"/>.
        /// </summary>
        public string? preExtrapolation;

        /// <summary>
        /// One of <see cref="TimelineClipExtrapolationModes"/>.
        /// </summary>
        public string? postExtrapolation;
    }
}
