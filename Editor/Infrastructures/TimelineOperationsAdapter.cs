using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class TimelineOperationsAdapter : ITimelineOperations
    {
        // TimelineClip exposes the extrapolation modes with internal setters only.
        private static readonly MethodInfo s_preExtrapolationSetter = typeof(TimelineClip)
            .GetProperty(nameof(TimelineClip.preExtrapolationMode))?.GetSetMethod(true);

        private static readonly MethodInfo s_postExtrapolationSetter = typeof(TimelineClip)
            .GetProperty(nameof(TimelineClip.postExtrapolationMode))?.GetSetMethod(true);

        public CreateTimelineResponse CreateTimeline(string assetPath)
        {
            var timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timelineAsset, assetPath);

            return new CreateTimelineResponse(true, assetPath);
        }

        public void AddTrack(int instanceId, string trackType, string trackName)
        {
            var director = GetPlayableDirector(instanceId);
            var timelineAsset = GetTimelineAsset(director, instanceId);

            var type = ResolveTrackType(trackType);
            Undo.RegisterCompleteObjectUndo(timelineAsset, "Add Timeline Track");
            timelineAsset.CreateTrack(type, null, trackName);
            EditorUtility.SetDirty(timelineAsset);
        }

        public void RemoveTrack(int instanceId, int trackIndex)
        {
            var director = GetPlayableDirector(instanceId);
            var timelineAsset = GetTimelineAsset(director, instanceId);
            var track = GetTrack(timelineAsset, trackIndex);

            Undo.RegisterCompleteObjectUndo(timelineAsset, "Remove Timeline Track");
            timelineAsset.DeleteTrack(track);
            EditorUtility.SetDirty(timelineAsset);
        }

        public void BindTrack(int instanceId, int trackIndex, int targetInstanceId)
        {
            var director = GetPlayableDirector(instanceId);
            var timelineAsset = GetTimelineAsset(director, instanceId);
            var track = GetTrack(timelineAsset, trackIndex);

            var targetObject = EditorUtility.InstanceIDToObject(targetInstanceId);
            if (targetObject == null)
            {
                throw new ArgumentException(
                    $"No object found with instanceId={targetInstanceId}.");
            }

            Undo.RecordObject(director, "Set Timeline Binding");
            director.SetGenericBinding(track, targetObject);
        }

        public void AddClip(int instanceId, int trackIndex, double start, double duration, string clipName)
        {
            var director = GetPlayableDirector(instanceId);
            var timelineAsset = GetTimelineAsset(director, instanceId);
            var track = GetTrack(timelineAsset, trackIndex);

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
            var timelineAsset = GetTimelineAsset(director, instanceId);
            var track = GetTrack(timelineAsset, trackIndex);
            var clip = GetClip(track, clipIndex);

            Undo.RegisterCompleteObjectUndo(timelineAsset, "Remove Timeline Clip");
            timelineAsset.DeleteClip(clip);
            EditorUtility.SetDirty(timelineAsset);
        }

        public GetTimelineTracksResponse GetTracks(int instanceId, string assetPath)
        {
            var timelineAsset = ResolveTimeline(instanceId, assetPath, out var director);

            var tracks = new List<TimelineTrackEntry>();
            var outputTracks = timelineAsset.GetOutputTracks().ToArray();
            for (var i = 0; i < outputTracks.Length; i++)
            {
                tracks.Add(CreateTrackEntry(outputTracks[i], i, director));
            }

            return new GetTimelineTracksResponse(AssetDatabase.GetAssetPath(timelineAsset), timelineAsset.duration,
                timelineAsset.editorSettings.frameRate, tracks);
        }

        public GetTimelineTrackPropertiesResponse GetTrackProperties(int instanceId, string assetPath, int trackIndex)
        {
            var timelineAsset = ResolveTimeline(instanceId, assetPath, out var director);
            var track = GetTrack(timelineAsset, trackIndex);

            return new GetTimelineTrackPropertiesResponse(GetTypeName(track),
                ReadTopLevelProperties(new SerializedObject(track, director)));
        }

        public GetTimelineClipPropertiesResponse GetClipProperties(int instanceId, string assetPath, int trackIndex,
            int clipIndex)
        {
            var timelineAsset = ResolveTimeline(instanceId, assetPath, out var director);
            var track = GetTrack(timelineAsset, trackIndex);
            var clip = GetClip(track, clipIndex);
            var asset = GetClipAsset(clip, trackIndex, clipIndex);

            // The director is the context that resolves ExposedReference properties.
            return new GetTimelineClipPropertiesResponse(GetTypeName(asset),
                ReadTopLevelProperties(new SerializedObject(asset, director)));
        }

        public void ModifyClip(ModifyTimelineClipRequest request)
        {
            var director = GetPlayableDirector(request.instanceId);
            var timelineAsset = GetTimelineAsset(director, request.instanceId);
            var track = GetTrack(timelineAsset, request.trackIndex);
            var clip = GetClip(track, request.clipIndex);

            ValidateClipChanges(clip, request);
            var preExtrapolation = ParseExtrapolation(request.preExtrapolation, nameof(request.preExtrapolation));
            var postExtrapolation = ParseExtrapolation(request.postExtrapolation, nameof(request.postExtrapolation));

            // TimelineClip is serialized inside its parent track.
            Undo.RegisterCompleteObjectUndo(track, "Set Timeline Clip");

            if (request.displayName != null)
            {
                clip.displayName = request.displayName;
            }

            if (request.start.HasValue)
            {
                clip.start = request.start.Value;
            }

            // Duration first, because the ease durations are clamped to it.
            if (request.duration.HasValue)
            {
                clip.duration = request.duration.Value;
            }

            if (request.timeScale.HasValue)
            {
                clip.timeScale = request.timeScale.Value;
            }

            if (request.clipIn.HasValue)
            {
                clip.clipIn = request.clipIn.Value;
            }

            if (request.easeInDuration.HasValue)
            {
                clip.easeInDuration = request.easeInDuration.Value;
            }

            if (request.easeOutDuration.HasValue)
            {
                clip.easeOutDuration = request.easeOutDuration.Value;
            }

            if (preExtrapolation.HasValue)
            {
                SetExtrapolation(s_preExtrapolationSetter, clip, preExtrapolation.Value);
            }

            if (postExtrapolation.HasValue)
            {
                SetExtrapolation(s_postExtrapolationSetter, clip, postExtrapolation.Value);
            }

            // Reading the track duration recalculates the extrapolation times of its clips
            // when their timing has changed.
            _ = track.duration;

            EditorUtility.SetDirty(track);
        }

        public void SetClipProperty(int instanceId, int trackIndex, int clipIndex, string propertyPath,
            string value)
        {
            var director = GetPlayableDirector(instanceId);
            var timelineAsset = GetTimelineAsset(director, instanceId);
            var track = GetTrack(timelineAsset, trackIndex);
            var clip = GetClip(track, clipIndex);
            var asset = GetClipAsset(clip, trackIndex, clipIndex);

            // The director is the context that resolves ExposedReference properties.
            var serializedObject = new SerializedObject(asset, director);
            ApplySerializedProperty(serializedObject, propertyPath, value, $"clip {clipIndex} on track {trackIndex}");
        }

        public void SetTrackProperty(int instanceId, int trackIndex, string propertyPath, string value)
        {
            var director = GetPlayableDirector(instanceId);
            var timelineAsset = GetTimelineAsset(director, instanceId);
            var track = GetTrack(timelineAsset, trackIndex);

            var serializedObject = new SerializedObject(track, director);
            ApplySerializedProperty(serializedObject, propertyPath, value, $"track {trackIndex}");
        }

        public void Play(int instanceId)
        {
            var director = GetPlayableDirector(instanceId);
            director.Play();
        }

        public void Stop(int instanceId)
        {
            var director = GetPlayableDirector(instanceId);
            director.Stop();
        }

        public void Evaluate(int instanceId, double time)
        {
            var director = GetPlayableDirector(instanceId);
            if (director.playableAsset == null)
            {
                throw new InvalidOperationException(
                    $"PlayableDirector (instanceId={instanceId}) has no PlayableAsset assigned.");
            }

            if (EditorApplication.isPlaying)
            {
                director.time = time;
                director.Evaluate();
                return;
            }

            // In Edit Mode, drive the Timeline window preview so that animated values are applied via
            // AnimationMode and reverted when the preview ends, instead of being written into the scene.
            var window = TimelineEditor.GetOrCreateWindow();
            if (TimelineEditor.masterDirector != director)
            {
                window.SetTimeline(director);
            }

            window.playbackControls.SetCurrentTime(time, TimelinePlaybackControls.Context.Global);
            TimelineEditor.Refresh(RefreshReason.SceneNeedsUpdate);
        }

        private static TimelineTrackEntry CreateTrackEntry(TrackAsset track, int index, PlayableDirector director)
        {
            var binding = director != null ? director.GetGenericBinding(track) : null;
            var clips = track.GetClips().ToArray();
            var clipEntries = new List<TimelineClipEntry>(clips.Length);
            for (var i = 0; i < clips.Length; i++)
            {
                clipEntries.Add(CreateClipEntry(clips[i], i));
            }

            return new TimelineTrackEntry
            {
                index = index,
                name = track.name,
                type = track.GetType().FullName,
                groupName = track.parent is TrackAsset parentTrack ? parentTrack.name : "",
                muted = track.muted,
                locked = track.locked,
                bindingInstanceId = binding != null ? binding.GetInstanceID() : 0,
                bindingName = binding != null ? binding.name : "",
                bindingType = binding != null ? binding.GetType().FullName : "",
                clips = clipEntries
            };
        }

        private static TimelineClipEntry CreateClipEntry(TimelineClip clip, int index)
        {
            var animationClip = clip.asset is AnimationPlayableAsset animationPlayableAsset
                ? animationPlayableAsset.clip
                : null;

            return new TimelineClipEntry
            {
                index = index,
                displayName = clip.displayName,
                assetType = GetTypeName(clip.asset),
                start = clip.start,
                duration = clip.duration,
                timeScale = clip.timeScale,
                clipIn = clip.clipIn,
                easeInDuration = clip.easeInDuration,
                easeOutDuration = clip.easeOutDuration,
                preExtrapolation = clip.preExtrapolationMode.ToString(),
                postExtrapolation = clip.postExtrapolationMode.ToString(),
                animationClipPath = animationClip != null ? AssetDatabase.GetAssetPath(animationClip) : ""
            };
        }

        private static List<SerializedPropertyEntry> ReadTopLevelProperties(SerializedObject serializedObject)
        {
            var properties = new List<SerializedPropertyEntry>();

            // enterChildren=true on the first call to enter the root,
            // then false to stay at the top level.
            var iterator = serializedObject.GetIterator();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyPath == "m_Script")
                {
                    continue;
                }

                properties.Add(new SerializedPropertyEntry(
                    iterator.propertyPath,
                    iterator.propertyType.ToString(),
                    SerializedPropertyValueConverter.ToValueString(iterator)));
            }

            return properties;
        }

        private static void ApplySerializedProperty(SerializedObject serializedObject, string propertyPath,
            string value, string targetDescription)
        {
            var property = serializedObject.FindProperty(propertyPath);
            if (property == null)
            {
                throw new ArgumentException($"Property '{propertyPath}' not found on {targetDescription}.");
            }

            // ApplyModifiedProperties records the change for Undo.
            SerializedPropertyValueParser.ApplyValue(property, value);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
        }

        private static void ValidateClipChanges(TimelineClip clip, ModifyTimelineClipRequest request)
        {
            ValidateTime(request.start, nameof(request.start), allowZero: true);
            ValidateTime(request.duration, nameof(request.duration), allowZero: false);
            ValidateTime(request.timeScale, nameof(request.timeScale), allowZero: false);
            ValidateTime(request.clipIn, nameof(request.clipIn), allowZero: true);
            ValidateTime(request.easeInDuration, nameof(request.easeInDuration), allowZero: true);
            ValidateTime(request.easeOutDuration, nameof(request.easeOutDuration), allowZero: true);

            RequireCaps(clip, ClipCaps.SpeedMultiplier, request.timeScale.HasValue, nameof(request.timeScale));
            RequireCaps(clip, ClipCaps.ClipIn, request.clipIn.HasValue, nameof(request.clipIn));
            RequireCaps(clip, ClipCaps.Blending,
                request.easeInDuration.HasValue || request.easeOutDuration.HasValue, "easeIn/easeOut");
            RequireCaps(clip, ClipCaps.Extrapolation,
                request.preExtrapolation != null || request.postExtrapolation != null, "extrapolation");
        }

        private static void ValidateTime(double? value, string name, bool allowZero)
        {
            if (!value.HasValue)
            {
                return;
            }

            var v = value.Value;
            if (double.IsNaN(v) || double.IsInfinity(v) || v < 0 || (!allowZero && v == 0))
            {
                throw new ArgumentException(
                    $"{name} must be {(allowZero ? "zero or positive" : "positive")}, but was {v}.");
            }
        }

        private static void RequireCaps(TimelineClip clip, ClipCaps caps, bool requested, string feature)
        {
            if (requested && (clip.clipCaps & caps) == 0)
            {
                throw new ArgumentException(
                    $"The clip (asset type: {GetTypeName(clip.asset)}) does not support {feature}.");
            }
        }

        private static TimelineClip.ClipExtrapolation? ParseExtrapolation(string value, string name)
        {
            if (value == null)
            {
                return null;
            }

            var names = Enum.GetNames(typeof(TimelineClip.ClipExtrapolation));
            var match = names.FirstOrDefault(n => string.Equals(n, value, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                throw new ArgumentException(
                    $"Invalid {name} '{value}'. Valid values: {string.Join(", ", names)}.");
            }

            return (TimelineClip.ClipExtrapolation)Enum.Parse(typeof(TimelineClip.ClipExtrapolation), match);
        }

        private static void SetExtrapolation(MethodInfo setter, TimelineClip clip,
            TimelineClip.ClipExtrapolation mode)
        {
            if (setter == null)
            {
                throw new InvalidOperationException(
                    "Setting the clip extrapolation is not supported by this Timeline package version.");
            }

            setter.Invoke(clip, new object[] { mode });
        }

        private static string GetTypeName(object obj)
        {
            return obj != null ? obj.GetType().FullName : "";
        }

        /// <summary>
        /// Resolves the timeline from a PlayableDirector (instanceId) or, when instanceId is 0, from an asset path.
        /// The director is null when resolved by asset path.
        /// </summary>
        private static TimelineAsset ResolveTimeline(int instanceId, string assetPath, out PlayableDirector director)
        {
            if (instanceId != 0)
            {
                director = GetPlayableDirector(instanceId);
                return GetTimelineAsset(director, instanceId);
            }

            director = null;
            var timelineAsset = AssetDatabase.LoadAssetAtPath<TimelineAsset>(assetPath);
            if (timelineAsset == null)
            {
                throw new ArgumentException($"TimelineAsset not found at path '{assetPath}'.");
            }

            return timelineAsset;
        }

        private static UnityEngine.Object GetClipAsset(TimelineClip clip, int trackIndex, int clipIndex)
        {
            if (clip.asset == null)
            {
                throw new InvalidOperationException(
                    $"Clip {clipIndex} on track {trackIndex} has no PlayableAsset.");
            }

            return clip.asset;
        }

        private static TimelineAsset GetTimelineAsset(PlayableDirector director, int instanceId)
        {
            var timelineAsset = director.playableAsset as TimelineAsset;
            if (timelineAsset == null)
            {
                throw new InvalidOperationException(
                    $"PlayableDirector (instanceId={instanceId}) has no TimelineAsset assigned.");
            }

            return timelineAsset;
        }

        private static TrackAsset GetTrack(TimelineAsset timelineAsset, int trackIndex)
        {
            var outputTracks = timelineAsset.GetOutputTracks().ToArray();
            if (trackIndex < 0 || trackIndex >= outputTracks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(trackIndex),
                    $"Track index {trackIndex} is out of range. The timeline has {outputTracks.Length} tracks.");
            }

            return outputTracks[trackIndex];
        }

        private static TimelineClip GetClip(TrackAsset track, int clipIndex)
        {
            var clips = track.GetClips().ToArray();
            if (clipIndex < 0 || clipIndex >= clips.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(clipIndex),
                    $"Clip index {clipIndex} is out of range. The track has {clips.Length} clips.");
            }

            return clips[clipIndex];
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
            var type = TypeCache.GetTypesDerivedFrom<TrackAsset>()
                .FirstOrDefault(t => t.FullName == trackType);

            if (type == null)
            {
                throw new ArgumentException(
                    $"Track type not found: '{trackType}'. " +
                    "Specify a fully qualified type name (e.g. UnityEngine.Timeline.AnimationTrack).");
            }

            return type;
        }
    }
}
