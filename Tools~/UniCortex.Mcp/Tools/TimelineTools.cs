using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class TimelineTools(TimelineUseCase timelineUseCase, IAsyncOperationSequencer sequencer)
{
    private const string TargetInstanceIdDescription =
        "The instanceId of a GameObject with a PlayableDirector component. 0 to use assetPath instead.";

    private const string TargetAssetPathDescription =
        "Asset path of the TimelineAsset (e.g. \"Assets/Timelines/MyTimeline.playable\"). Used when instanceId is 0.";

    private const string ExtrapolationDescription =
        "Extrapolation mode (None, Hold, Loop, PingPong, Continue)";

    private const string PropertyValueDescription =
        "The new value as a string, in the same format as the get_timeline_*_properties tools return " +
        "(e.g. \"true\", \"1.5\", \"(1, 2, 3)\", an enum display name such as \"Apply Scene Offsets\", " +
        "or \"null\"). " + PropertyValueDescriptions.ObjectReference;

    [McpServerTool(Name = "create_timeline", ReadOnly = false),
     Description(
         "Create a new TimelineAsset (.playable file) at the specified asset path."),
     UsedImplicitly]
    public ValueTask<CallToolResult> CreateTimelineAsync(
        [Description("Asset path where the TimelineAsset will be saved (e.g. \"Assets/Timelines/MyTimeline.playable\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.CreateAsync(assetPath, ct), cancellationToken);

    [McpServerTool(Name = "add_timeline_track", ReadOnly = false),
     Description(
         "Add a track to a TimelineAsset on a PlayableDirector. Undo supported."),
     UsedImplicitly]
    public ValueTask<CallToolResult> AddTimelineTrackAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description(
            "Fully qualified type name of the track to add " +
            "(e.g. UnityEngine.Timeline.AnimationTrack, UnityEngine.Timeline.AudioTrack, " +
            "UnityEngine.Timeline.ActivationTrack, UnityEngine.Timeline.ControlTrack, " +
            "UnityEngine.Timeline.SignalTrack, UnityEngine.Timeline.GroupTrack).")]
        string trackType,
        [Description("Optional name for the new track.")]
        string trackName = "",
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.AddTrackAsync(instanceId, trackType, trackName, ct), cancellationToken);

    [McpServerTool(Name = "remove_timeline_track", ReadOnly = false),
     Description(
         "Remove a track from a TimelineAsset on a PlayableDirector by index. Undo supported."),
     UsedImplicitly]
    public ValueTask<CallToolResult> RemoveTimelineTrackAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description("The index of the track to remove (0-based).")]
        int trackIndex,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.RemoveTrackAsync(instanceId, trackIndex, ct), cancellationToken);

    [McpServerTool(Name = "bind_timeline_track", ReadOnly = false),
     Description(
         "Set the binding of a Timeline track on a PlayableDirector. Undo supported."),
     UsedImplicitly]
    public ValueTask<CallToolResult> BindTimelineTrackAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description("The index of the track to bind (0-based).")]
        int trackIndex,
        [Description("The instanceId of the target object to bind to the track.")]
        int targetInstanceId,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.BindTrackAsync(instanceId, trackIndex, targetInstanceId, ct),
            cancellationToken);

    [McpServerTool(Name = "add_timeline_clip", ReadOnly = false),
     Description(
         "Add a default clip to a Timeline track. The clip type is determined by the track type. Undo supported."),
     UsedImplicitly]
    public ValueTask<CallToolResult> AddTimelineClipAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description("The index of the track to add the clip to (0-based).")]
        int trackIndex,
        [Description("Start time of the clip in seconds.")]
        double start = 0,
        [Description("Duration of the clip in seconds. 0 uses the track's default duration.")]
        double duration = 0,
        [Description("Optional display name for the clip.")]
        string clipName = "",
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.AddClipAsync(instanceId, trackIndex, start, duration, clipName, ct),
            cancellationToken);

    [McpServerTool(Name = "remove_timeline_clip", ReadOnly = false),
     Description(
         "Remove a clip from a Timeline track by index. Undo supported."),
     UsedImplicitly]
    public ValueTask<CallToolResult> RemoveTimelineClipAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description("The index of the track containing the clip (0-based).")]
        int trackIndex,
        [Description("The index of the clip to remove within the track (0-based).")]
        int clipIndex,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.RemoveClipAsync(instanceId, trackIndex, clipIndex, ct),
            cancellationToken);

    [McpServerTool(Name = "get_timeline_tracks", ReadOnly = true),
     Description(
         "Get an overview of the tracks and clips of a Timeline: track type, group, binding and muted state, " +
         "and for each clip its start, duration, timeScale, clipIn, ease in/out, extrapolation and the AnimationClip it uses. " +
         "The returned track/clip indices are the trackIndex/clipIndex used by the other Timeline tools. " +
         "Use get_timeline_track_properties / get_timeline_clip_properties for serialized properties. " +
         "Specify either instanceId or assetPath. Bindings are only resolved with instanceId."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetTimelineTracksAsync(
        [Description(TargetInstanceIdDescription)]
        int instanceId = 0,
        [Description(TargetAssetPathDescription)]
        string? assetPath = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.GetTracksAsync(instanceId, assetPath, ct), cancellationToken);

    [McpServerTool(Name = "get_timeline_track_properties", ReadOnly = true),
     Description(
         "Get the top-level serialized properties of a Timeline track (e.g. m_TrackOffset of an Animation track). " +
         "The paths and values can be passed to set_timeline_track_property. " +
         "Hidden properties such as m_Muted are not listed but can still be set. " +
         "Specify either instanceId or assetPath."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetTimelineTrackPropertiesAsync(
        [Description("The index of the track (0-based).")]
        int trackIndex,
        [Description(TargetInstanceIdDescription)]
        int instanceId = 0,
        [Description(TargetAssetPathDescription)]
        string? assetPath = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.GetTrackPropertiesAsync(instanceId, assetPath, trackIndex, ct),
            cancellationToken);

    [McpServerTool(Name = "get_timeline_clip_properties", ReadOnly = true),
     Description(
         "Get the top-level serialized properties of the content (PlayableAsset) of a Timeline clip " +
         "(e.g. m_Clip of an Animation clip, or the settings of a custom clip). " +
         "The paths and values can be passed to set_timeline_clip_property. " +
         "Specify either instanceId or assetPath. ExposedReference values are only resolved with instanceId."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetTimelineClipPropertiesAsync(
        [Description("The index of the track containing the clip (0-based).")]
        int trackIndex,
        [Description("The index of the clip within the track (0-based).")]
        int clipIndex,
        [Description(TargetInstanceIdDescription)]
        int instanceId = 0,
        [Description(TargetAssetPathDescription)]
        string? assetPath = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.GetClipPropertiesAsync(instanceId, assetPath, trackIndex, clipIndex, ct),
            cancellationToken);

    [McpServerTool(Name = "modify_timeline_clip", ReadOnly = false),
     Description(
         "Change the timing and settings of a Timeline clip. Only the specified values are changed. " +
         "timeScale, clipIn, ease in/out and extrapolation fail if the clip type does not support them. " +
         "Undo supported."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ModifyTimelineClipAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description("The index of the track containing the clip (0-based).")]
        int trackIndex,
        [Description("The index of the clip within the track (0-based).")]
        int clipIndex,
        [Description("Start time of the clip in seconds (>= 0).")]
        double? start = null,
        [Description("Duration of the clip in seconds (> 0).")]
        double? duration = null,
        [Description("Speed multiplier of the clip (> 0).")]
        double? timeScale = null,
        [Description("Offset in seconds into the clip's source where playback starts (>= 0).")]
        double? clipIn = null,
        [Description("Ease in duration in seconds (>= 0).")]
        double? easeInDuration = null,
        [Description("Ease out duration in seconds (>= 0).")]
        double? easeOutDuration = null,
        [Description(ExtrapolationDescription + " applied before the clip starts.")]
        string? preExtrapolation = null,
        [Description(ExtrapolationDescription + " applied after the clip ends.")]
        string? postExtrapolation = null,
        [Description("Display name of the clip.")]
        string? displayName = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.ModifyClipAsync(new ModifyTimelineClipRequest
            {
                instanceId = instanceId,
                trackIndex = trackIndex,
                clipIndex = clipIndex,
                displayName = displayName,
                start = start,
                duration = duration,
                timeScale = timeScale,
                clipIn = clipIn,
                easeInDuration = easeInDuration,
                easeOutDuration = easeOutDuration,
                preExtrapolation = preExtrapolation,
                postExtrapolation = postExtrapolation
            }, ct),
            cancellationToken);

    [McpServerTool(Name = "set_timeline_clip_property", ReadOnly = false),
     Description(
         "Set a serialized property on the content (PlayableAsset) of a Timeline clip, " +
         "e.g. m_Clip to assign an AnimationClip to an Animation clip, or the settings of a custom clip. " +
         "Property paths are listed by get_timeline_clip_properties. " +
         "The clip's own timing (start, duration, ease, extrapolation) is changed with modify_timeline_clip. " +
         "ExposedReference properties are resolved through the PlayableDirector. Undo supported."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetTimelineClipPropertyAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description("The index of the track containing the clip (0-based).")]
        int trackIndex,
        [Description("The index of the clip within the track (0-based).")]
        int clipIndex,
        [Description("The serialized property path (e.g. \"m_Clip\", \"postPlayback\", \"template.speed\").")]
        string propertyPath,
        [Description(PropertyValueDescription)]
        string value,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.SetClipPropertyAsync(instanceId, trackIndex, clipIndex, propertyPath, value,
                ct),
            cancellationToken);

    [McpServerTool(Name = "set_timeline_track_property", ReadOnly = false),
     Description(
         "Set a serialized property on a Timeline track (e.g. Track Offsets of an Animation track, or m_Muted to mute it). " +
         "Property paths are listed by get_timeline_track_properties. Undo supported."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetTimelineTrackPropertyAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description("The index of the track (0-based).")]
        int trackIndex,
        [Description("The serialized property path (e.g. \"m_Muted\", \"m_TrackOffset\", \"m_Position\").")]
        string propertyPath,
        [Description(PropertyValueDescription)]
        string value,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.SetTrackPropertyAsync(instanceId, trackIndex, propertyPath, value, ct),
            cancellationToken);

    [McpServerTool(Name = "play_timeline", ReadOnly = false),
     Description(
         "Start playback of a Timeline on a PlayableDirector."),
     UsedImplicitly]
    public ValueTask<CallToolResult> PlayTimelineAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.PlayAsync(instanceId, ct), cancellationToken);

    [McpServerTool(Name = "stop_timeline", ReadOnly = false),
     Description(
         "Stop playback of a Timeline on a PlayableDirector and reset to the beginning."),
     UsedImplicitly]
    public ValueTask<CallToolResult> StopTimelineAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.StopAsync(instanceId, ct), cancellationToken);
}
