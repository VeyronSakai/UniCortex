using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface ITimelineOperations
    {
        TimelineInfoResponse GetTimelineInfo(int instanceId);
        void SetTimelineTime(int instanceId, double time);
        void PlayTimeline(int instanceId);
        void PauseTimeline(int instanceId);
        void StopTimeline(int instanceId);
        void AddTrack(int instanceId, string trackType, string trackName);
        void RemoveTrack(int instanceId, int trackIndex);
        void SetBinding(int instanceId, int trackIndex, int targetInstanceId);
    }
}
