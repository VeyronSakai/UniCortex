using System;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Infrastructures
{
    // Fallback used when the Timeline package (com.unity.timeline) is not installed.
    internal sealed class TimelineNotSupportedAdapter : ITimelineOperations
    {
        public CreateTimelineResponse CreateTimeline(string assetPath)
        {
            throw new NotSupportedException(
                "Timeline package (com.unity.timeline) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }

        public void AddTrack(int instanceId, string trackType, string trackName)
        {
            throw new NotSupportedException(
                "Timeline package (com.unity.timeline) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }

        public void RemoveTrack(int instanceId, int trackIndex)
        {
            throw new NotSupportedException(
                "Timeline package (com.unity.timeline) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }

        public void SetBinding(int instanceId, int trackIndex, int targetInstanceId)
        {
            throw new NotSupportedException(
                "Timeline package (com.unity.timeline) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }

        public void AddClip(int instanceId, int trackIndex, double start, double duration, string clipName)
        {
            throw new NotSupportedException(
                "Timeline package (com.unity.timeline) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }

        public void RemoveClip(int instanceId, int trackIndex, int clipIndex)
        {
            throw new NotSupportedException(
                "Timeline package (com.unity.timeline) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }
    }
}
