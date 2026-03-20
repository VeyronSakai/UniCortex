using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyTimelineOperations : ITimelineOperations
    {
        public int CreateTimelineCallCount { get; private set; }
        public string LastCreateAssetPath { get; private set; }

        public int AddTrackCallCount { get; private set; }
        public int LastAddTrackInstanceId { get; private set; }
        public string LastAddTrackType { get; private set; }
        public string LastAddTrackName { get; private set; }

        public int RemoveTrackCallCount { get; private set; }
        public int LastRemoveTrackInstanceId { get; private set; }
        public int LastRemoveTrackIndex { get; private set; }

        public int SetBindingCallCount { get; private set; }
        public int LastSetBindingInstanceId { get; private set; }
        public int LastSetBindingTrackIndex { get; private set; }
        public int LastSetBindingTargetInstanceId { get; private set; }

        public int AddClipCallCount { get; private set; }
        public int LastAddClipInstanceId { get; private set; }
        public int LastAddClipTrackIndex { get; private set; }
        public double LastAddClipStart { get; private set; }
        public double LastAddClipDuration { get; private set; }
        public string LastAddClipName { get; private set; }

        public int RemoveClipCallCount { get; private set; }
        public int LastRemoveClipInstanceId { get; private set; }
        public int LastRemoveClipTrackIndex { get; private set; }
        public int LastRemoveClipIndex { get; private set; }

        public CreateTimelineResponse CreateTimeline(string assetPath)
        {
            CreateTimelineCallCount++;
            LastCreateAssetPath = assetPath;
            return new CreateTimelineResponse(true, assetPath);
        }

        public void AddTrack(int instanceId, string trackType, string trackName)
        {
            AddTrackCallCount++;
            LastAddTrackInstanceId = instanceId;
            LastAddTrackType = trackType;
            LastAddTrackName = trackName;
        }

        public void RemoveTrack(int instanceId, int trackIndex)
        {
            RemoveTrackCallCount++;
            LastRemoveTrackInstanceId = instanceId;
            LastRemoveTrackIndex = trackIndex;
        }

        public void SetBinding(int instanceId, int trackIndex, int targetInstanceId)
        {
            SetBindingCallCount++;
            LastSetBindingInstanceId = instanceId;
            LastSetBindingTrackIndex = trackIndex;
            LastSetBindingTargetInstanceId = targetInstanceId;
        }

        public void AddClip(int instanceId, int trackIndex, double start, double duration, string clipName)
        {
            AddClipCallCount++;
            LastAddClipInstanceId = instanceId;
            LastAddClipTrackIndex = trackIndex;
            LastAddClipStart = start;
            LastAddClipDuration = duration;
            LastAddClipName = clipName;
        }

        public void RemoveClip(int instanceId, int trackIndex, int clipIndex)
        {
            RemoveClipCallCount++;
            LastRemoveClipInstanceId = instanceId;
            LastRemoveClipTrackIndex = trackIndex;
            LastRemoveClipIndex = clipIndex;
        }
    }
}
