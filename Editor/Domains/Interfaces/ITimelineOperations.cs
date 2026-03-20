using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface ITimelineOperations
    {
        CreateTimelineResponse CreateTimeline(string assetPath);
        TimelineInfoResponse GetTimelineInfo(int instanceId);
        void AddTrack(int instanceId, string trackType, string trackName);
        void RemoveTrack(int instanceId, int trackIndex);
        void SetBinding(int instanceId, int trackIndex, int targetInstanceId);
        void AddClip(int instanceId, int trackIndex, double start, double duration, string clipName);
        void RemoveClip(int instanceId, int trackIndex, int clipIndex);
    }
}
