#if UNICORTEX_TIMELINE
using System;
using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class TimelineOperationsAdapter : ITimelineOperations
    {
        public CreateTimelineResponse CreateTimeline(int instanceId, string assetPath)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj == null)
            {
                throw new ArgumentException($"No object found with instanceId={instanceId}.");
            }

            var go = obj as GameObject;
            if (go == null)
            {
                throw new InvalidOperationException(
                    $"Object with instanceId={instanceId} is not a GameObject.");
            }

            var timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timelineAsset, assetPath);

            var director = go.GetComponent<PlayableDirector>();
            if (director == null)
            {
                director = Undo.AddComponent<PlayableDirector>(go);
            }
            else
            {
                Undo.RecordObject(director, "Assign Timeline Asset");
            }

            director.playableAsset = timelineAsset;

            return new CreateTimelineResponse(assetPath, go.GetInstanceID());
        }

        public TimelineInfoResponse GetTimelineInfo(int instanceId)
        {
            var director = GetPlayableDirector(instanceId);
            var timelineAsset = director.playableAsset as TimelineAsset;
            if (timelineAsset == null)
            {
                throw new InvalidOperationException(
                    $"PlayableDirector (instanceId={instanceId}) has no TimelineAsset assigned.");
            }

            var outputTracks = timelineAsset.GetOutputTracks().ToArray();

            var tracks = new List<TimelineTrackInfo>();
            var bindings = new List<TimelineBindingInfo>();

            for (var i = 0; i < outputTracks.Length; i++)
            {
                var track = outputTracks[i];

                var clips = new List<TimelineClipInfo>();
                foreach (var clip in track.GetClips())
                {
                    clips.Add(new TimelineClipInfo
                    {
                        name = clip.displayName,
                        start = clip.start,
                        duration = clip.duration,
                        end = clip.end
                    });
                }

                tracks.Add(new TimelineTrackInfo
                {
                    name = track.name,
                    type = track.GetType().Name,
                    muted = track.muted,
                    locked = track.locked,
                    clips = clips.ToArray()
                });

                var binding = director.GetGenericBinding(track);
                bindings.Add(new TimelineBindingInfo
                {
                    trackIndex = i,
                    trackName = track.name,
                    sourceType = track.GetType().Name,
                    boundObjectName = binding != null ? binding.name : "",
                    boundObjectInstanceId = binding != null ? binding.GetInstanceID() : 0
                });
            }

            return new TimelineInfoResponse
            {
                timelineAssetName = timelineAsset.name,
                duration = timelineAsset.duration,
                currentTime = director.time,
                isPlaying = director.state == PlayState.Playing,
                tracks = tracks.ToArray(),
                bindings = bindings.ToArray()
            };
        }

        public void AddTrack(int instanceId, string trackType, string trackName)
        {
            var director = GetPlayableDirector(instanceId);
            var timelineAsset = director.playableAsset as TimelineAsset;
            if (timelineAsset == null)
            {
                throw new InvalidOperationException(
                    $"PlayableDirector (instanceId={instanceId}) has no TimelineAsset assigned.");
            }

            var type = ResolveTrackType(trackType);
            Undo.RegisterCompleteObjectUndo(timelineAsset, "Add Timeline Track");
            timelineAsset.CreateTrack(type, null, trackName);
            EditorUtility.SetDirty(timelineAsset);
        }

        public void RemoveTrack(int instanceId, int trackIndex)
        {
            var director = GetPlayableDirector(instanceId);
            var timelineAsset = director.playableAsset as TimelineAsset;
            if (timelineAsset == null)
            {
                throw new InvalidOperationException(
                    $"PlayableDirector (instanceId={instanceId}) has no TimelineAsset assigned.");
            }

            var outputTracks = timelineAsset.GetOutputTracks().ToArray();
            if (trackIndex < 0 || trackIndex >= outputTracks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(trackIndex),
                    $"Track index {trackIndex} is out of range. The timeline has {outputTracks.Length} tracks.");
            }

            Undo.RegisterCompleteObjectUndo(timelineAsset, "Remove Timeline Track");
            timelineAsset.DeleteTrack(outputTracks[trackIndex]);
            EditorUtility.SetDirty(timelineAsset);
        }

        public void SetBinding(int instanceId, int trackIndex, int targetInstanceId)
        {
            var director = GetPlayableDirector(instanceId);
            var timelineAsset = director.playableAsset as TimelineAsset;
            if (timelineAsset == null)
            {
                throw new InvalidOperationException(
                    $"PlayableDirector (instanceId={instanceId}) has no TimelineAsset assigned.");
            }

            var outputTracks = timelineAsset.GetOutputTracks().ToArray();
            if (trackIndex < 0 || trackIndex >= outputTracks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(trackIndex),
                    $"Track index {trackIndex} is out of range. The timeline has {outputTracks.Length} tracks.");
            }

            var targetObject = EditorUtility.InstanceIDToObject(targetInstanceId);
            if (targetObject == null)
            {
                throw new ArgumentException(
                    $"No object found with instanceId={targetInstanceId}.");
            }

            Undo.RecordObject(director, "Set Timeline Binding");
            director.SetGenericBinding(outputTracks[trackIndex], targetObject);
        }

        public void AddClip(int instanceId, int trackIndex, double start, double duration, string clipName)
        {
            var director = GetPlayableDirector(instanceId);
            var timelineAsset = director.playableAsset as TimelineAsset;
            if (timelineAsset == null)
            {
                throw new InvalidOperationException(
                    $"PlayableDirector (instanceId={instanceId}) has no TimelineAsset assigned.");
            }

            var outputTracks = timelineAsset.GetOutputTracks().ToArray();
            if (trackIndex < 0 || trackIndex >= outputTracks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(trackIndex),
                    $"Track index {trackIndex} is out of range. The timeline has {outputTracks.Length} tracks.");
            }

            var track = outputTracks[trackIndex];
            Undo.RegisterCompleteObjectUndo(timelineAsset, "Add Timeline Clip");
            var clip = track.CreateDefaultClip();
            clip.start = start;
            if (duration > 0)
            {
                clip.duration = duration;
            }

            if (!string.IsNullOrEmpty(clipName))
            {
                clip.displayName = clipName;
            }

            EditorUtility.SetDirty(timelineAsset);
        }

        public void RemoveClip(int instanceId, int trackIndex, int clipIndex)
        {
            var director = GetPlayableDirector(instanceId);
            var timelineAsset = director.playableAsset as TimelineAsset;
            if (timelineAsset == null)
            {
                throw new InvalidOperationException(
                    $"PlayableDirector (instanceId={instanceId}) has no TimelineAsset assigned.");
            }

            var outputTracks = timelineAsset.GetOutputTracks().ToArray();
            if (trackIndex < 0 || trackIndex >= outputTracks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(trackIndex),
                    $"Track index {trackIndex} is out of range. The timeline has {outputTracks.Length} tracks.");
            }

            var track = outputTracks[trackIndex];
            var clips = track.GetClips().ToArray();
            if (clipIndex < 0 || clipIndex >= clips.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(clipIndex),
                    $"Clip index {clipIndex} is out of range. The track has {clips.Length} clips.");
            }

            Undo.RegisterCompleteObjectUndo(timelineAsset, "Remove Timeline Clip");
            timelineAsset.DeleteClip(clips[clipIndex]);
            EditorUtility.SetDirty(timelineAsset);
        }

        private static PlayableDirector GetPlayableDirector(int instanceId)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj == null)
            {
                throw new ArgumentException(
                    $"No object found with instanceId={instanceId}.");
            }

            if (obj is GameObject go)
            {
                var director = go.GetComponent<PlayableDirector>();
                if (director == null)
                {
                    throw new InvalidOperationException(
                        $"GameObject '{go.name}' does not have a PlayableDirector component.");
                }

                return director;
            }

            if (obj is PlayableDirector d)
            {
                return d;
            }

            throw new InvalidOperationException(
                $"Object with instanceId={instanceId} is not a GameObject or PlayableDirector.");
        }

        private static Type ResolveTrackType(string trackType)
        {
            switch (trackType)
            {
                case TimelineTrackType.AnimationTrack:
                    return typeof(AnimationTrack);
                case TimelineTrackType.AudioTrack:
                    return typeof(AudioTrack);
                case TimelineTrackType.ActivationTrack:
                    return typeof(ActivationTrack);
                case TimelineTrackType.ControlTrack:
                    return typeof(ControlTrack);
                case TimelineTrackType.SignalTrack:
                    return typeof(SignalTrack);
                case TimelineTrackType.GroupTrack:
                    return typeof(GroupTrack);
                default:
                    throw new ArgumentException(
                        $"Unknown track type: '{trackType}'. Supported types: " +
                        $"{TimelineTrackType.AnimationTrack}, {TimelineTrackType.AudioTrack}, " +
                        $"{TimelineTrackType.ActivationTrack}, {TimelineTrackType.ControlTrack}, " +
                        $"{TimelineTrackType.SignalTrack}, {TimelineTrackType.GroupTrack}.");
            }
        }
    }
}
#endif
