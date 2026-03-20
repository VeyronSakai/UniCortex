using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyTimelineOperations : ITimelineOperations
    {
        public int GetTimelineInfoCallCount { get; private set; }
        public int LastGetInfoInstanceId { get; private set; }
        public TimelineInfoResponse GetTimelineInfoResult { get; set; }

        public int SetTimelineTimeCallCount { get; private set; }
        public int LastSetTimeInstanceId { get; private set; }
        public double LastSetTimeValue { get; private set; }

        public int PlayTimelineCallCount { get; private set; }
        public int LastPlayInstanceId { get; private set; }

        public int PauseTimelineCallCount { get; private set; }
        public int LastPauseInstanceId { get; private set; }

        public int StopTimelineCallCount { get; private set; }
        public int LastStopInstanceId { get; private set; }

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

        public TimelineInfoResponse GetTimelineInfo(int instanceId)
        {
            GetTimelineInfoCallCount++;
            LastGetInfoInstanceId = instanceId;
            return GetTimelineInfoResult ?? new TimelineInfoResponse
            {
                timelineAssetName = "TestTimeline",
                duration = 10.0,
                currentTime = 0.0,
                isPlaying = false,
                tracks = new TimelineTrackInfo[0],
                bindings = new TimelineBindingInfo[0]
            };
        }

        public void SetTimelineTime(int instanceId, double time)
        {
            SetTimelineTimeCallCount++;
            LastSetTimeInstanceId = instanceId;
            LastSetTimeValue = time;
        }

        public void PlayTimeline(int instanceId)
        {
            PlayTimelineCallCount++;
            LastPlayInstanceId = instanceId;
        }

        public void PauseTimeline(int instanceId)
        {
            PauseTimelineCallCount++;
            LastPauseInstanceId = instanceId;
        }

        public void StopTimeline(int instanceId)
        {
            StopTimelineCallCount++;
            LastStopInstanceId = instanceId;
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
    }
}
