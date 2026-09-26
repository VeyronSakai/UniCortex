using System.Collections.Generic;
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

        public int BindTrackCallCount { get; private set; }
        public int LastBindTrackInstanceId { get; private set; }
        public int LastBindTrackTrackIndex { get; private set; }
        public int LastBindTrackTargetInstanceId { get; private set; }

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

        public int GetTracksCallCount { get; private set; }
        public int LastGetTracksInstanceId { get; private set; }
        public string LastGetTracksAssetPath { get; private set; }
        public GetTimelineTracksResponse GetTracksResult { get; set; } =
            new GetTimelineTracksResponse("Assets/Test.playable", 0, 60, new List<TimelineTrackEntry>());

        public int GetTrackPropertiesCallCount { get; private set; }
        public int LastGetTrackPropertiesInstanceId { get; private set; }
        public string LastGetTrackPropertiesAssetPath { get; private set; }
        public int LastGetTrackPropertiesTrackIndex { get; private set; }
        public GetTimelineTrackPropertiesResponse GetTrackPropertiesResult { get; set; } =
            new GetTimelineTrackPropertiesResponse("UnityEngine.Timeline.AnimationTrack",
                new List<SerializedPropertyEntry>());

        public int GetClipPropertiesCallCount { get; private set; }
        public int LastGetClipPropertiesInstanceId { get; private set; }
        public string LastGetClipPropertiesAssetPath { get; private set; }
        public int LastGetClipPropertiesTrackIndex { get; private set; }
        public int LastGetClipPropertiesClipIndex { get; private set; }
        public GetTimelineClipPropertiesResponse GetClipPropertiesResult { get; set; } =
            new GetTimelineClipPropertiesResponse("UnityEngine.Timeline.AnimationPlayableAsset",
                new List<SerializedPropertyEntry>());

        public int ModifyClipCallCount { get; private set; }
        public ModifyTimelineClipRequest LastModifyClipRequest { get; private set; }

        public int SetClipPropertyCallCount { get; private set; }
        public int LastSetClipPropertyInstanceId { get; private set; }
        public int LastSetClipPropertyTrackIndex { get; private set; }
        public int LastSetClipPropertyClipIndex { get; private set; }
        public string LastSetClipPropertyPath { get; private set; }
        public string LastSetClipPropertyValue { get; private set; }

        public int SetTrackPropertyCallCount { get; private set; }
        public int LastSetTrackPropertyInstanceId { get; private set; }
        public int LastSetTrackPropertyTrackIndex { get; private set; }
        public string LastSetTrackPropertyPath { get; private set; }
        public string LastSetTrackPropertyValue { get; private set; }

        public int PlayCallCount { get; private set; }
        public int LastPlayInstanceId { get; private set; }

        public int StopCallCount { get; private set; }
        public int LastStopInstanceId { get; private set; }

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

        public void BindTrack(int instanceId, int trackIndex, int targetInstanceId)
        {
            BindTrackCallCount++;
            LastBindTrackInstanceId = instanceId;
            LastBindTrackTrackIndex = trackIndex;
            LastBindTrackTargetInstanceId = targetInstanceId;
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

        public GetTimelineTracksResponse GetTracks(int instanceId, string assetPath)
        {
            GetTracksCallCount++;
            LastGetTracksInstanceId = instanceId;
            LastGetTracksAssetPath = assetPath;
            return GetTracksResult;
        }

        public GetTimelineTrackPropertiesResponse GetTrackProperties(int instanceId, string assetPath, int trackIndex)
        {
            GetTrackPropertiesCallCount++;
            LastGetTrackPropertiesInstanceId = instanceId;
            LastGetTrackPropertiesAssetPath = assetPath;
            LastGetTrackPropertiesTrackIndex = trackIndex;
            return GetTrackPropertiesResult;
        }

        public GetTimelineClipPropertiesResponse GetClipProperties(int instanceId, string assetPath, int trackIndex,
            int clipIndex)
        {
            GetClipPropertiesCallCount++;
            LastGetClipPropertiesInstanceId = instanceId;
            LastGetClipPropertiesAssetPath = assetPath;
            LastGetClipPropertiesTrackIndex = trackIndex;
            LastGetClipPropertiesClipIndex = clipIndex;
            return GetClipPropertiesResult;
        }

        public void ModifyClip(ModifyTimelineClipRequest request)
        {
            ModifyClipCallCount++;
            LastModifyClipRequest = request;
        }

        public void SetClipProperty(int instanceId, int trackIndex, int clipIndex, string propertyPath,
            string value)
        {
            SetClipPropertyCallCount++;
            LastSetClipPropertyInstanceId = instanceId;
            LastSetClipPropertyTrackIndex = trackIndex;
            LastSetClipPropertyClipIndex = clipIndex;
            LastSetClipPropertyPath = propertyPath;
            LastSetClipPropertyValue = value;
        }

        public void SetTrackProperty(int instanceId, int trackIndex, string propertyPath, string value)
        {
            SetTrackPropertyCallCount++;
            LastSetTrackPropertyInstanceId = instanceId;
            LastSetTrackPropertyTrackIndex = trackIndex;
            LastSetTrackPropertyPath = propertyPath;
            LastSetTrackPropertyValue = value;
        }

        public void Play(int instanceId)
        {
            PlayCallCount++;
            LastPlayInstanceId = instanceId;
        }

        public void Stop(int instanceId)
        {
            StopCallCount++;
            LastStopInstanceId = instanceId;
        }
    }
}
