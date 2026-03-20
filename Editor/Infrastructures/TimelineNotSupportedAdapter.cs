using System;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Infrastructures
{
    // Fallback used when the Timeline package (com.unity.timeline) is not installed.
    internal sealed class TimelineNotSupportedAdapter : ITimelineOperations
    {
        public TimelineInfoResponse GetTimelineInfo(int instanceId)
        {
            throw new NotSupportedException(
                "Timeline package (com.unity.timeline) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }

        public void SetTimelineTime(int instanceId, double time)
        {
            throw new NotSupportedException(
                "Timeline package (com.unity.timeline) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }

        public void PlayTimeline(int instanceId)
        {
            throw new NotSupportedException(
                "Timeline package (com.unity.timeline) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }

        public void PauseTimeline(int instanceId)
        {
            throw new NotSupportedException(
                "Timeline package (com.unity.timeline) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }

        public void StopTimeline(int instanceId)
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
    }
}
