using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface ITimelineOperations
    {
        CreateTimelineResponse CreateTimeline(string assetPath);
        void AddTrack(int instanceId, string trackType, string trackName);
        void RemoveTrack(int instanceId, int trackIndex);
        void BindTrack(int instanceId, int trackIndex, int targetInstanceId);
        void AddClip(int instanceId, int trackIndex, double start, double duration, string clipName);
        void RemoveClip(int instanceId, int trackIndex, int clipIndex);
        void Play(int instanceId);
        void Stop(int instanceId);
    }
}
