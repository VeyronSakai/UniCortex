using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class TimelineClipEntry
    {
        /// <summary>
        /// Index used as clipIndex by the other Timeline tools.
        /// </summary>
        public int index;

        public string displayName;

        /// <summary>
        /// Full type name of the clip's PlayableAsset (e.g. UnityEngine.Timeline.AnimationPlayableAsset).
        /// </summary>
        public string assetType;

        public double start;
        public double duration;
        public double timeScale;
        public double clipIn;
        public double easeInDuration;
        public double easeOutDuration;

        /// <summary>
        /// One of <see cref="TimelineClipExtrapolationModes"/>.
        /// </summary>
        public string preExtrapolation;

        /// <summary>
        /// One of <see cref="TimelineClipExtrapolationModes"/>.
        /// </summary>
        public string postExtrapolation;

        /// <summary>
        /// Asset path of the AnimationClip used by the clip. Empty when the clip does not use one.
        /// </summary>
        public string animationClipPath;
    }
}
