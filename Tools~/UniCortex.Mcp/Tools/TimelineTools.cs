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
    private const string DirectorInstanceIdDescription =
        "instanceId of a GameObject with a PlayableDirector.";

    private const string TrackIndexDescription = "Track index (0-based).";

    private const string ClipIndexDescription = "Clip index within the track (0-based).";

    private const string TargetInstanceIdDescription =
        "instanceId of a GameObject with a PlayableDirector, or 0 to use assetPath.";

    private const string TargetAssetPathDescription =
        "TimelineAsset path (e.g. \"Assets/Timelines/Intro.playable\"), used when instanceId is 0.";

    private const string ExtrapolationDescription = "None, Hold, Loop, PingPong or Continue";

    private const string PropertyValueDescription =
        "New value in the format the get_timeline_*_properties tools return " +
        "(e.g. \"true\", \"1.5\", \"(1, 2, 3)\", an enum display name such as \"Apply Scene Offsets\"). " +
        PropertyValueDescriptions.ObjectReference;

    [McpServerTool(Name = "create_timeline", ReadOnly = false),
     Description("Create a new TimelineAsset (.playable file) at the asset path."),
     UsedImplicitly]
    public ValueTask<CallToolResult> CreateTimelineAsync(
        [Description("Asset path to save to (e.g. \"Assets/Timelines/Intro.playable\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.CreateAsync(assetPath, ct), cancellationToken);

    [McpServerTool(Name = "add_timeline_track", ReadOnly = false),
     Description("Add a track to the TimelineAsset of a PlayableDirector."),
     UsedImplicitly]
    public ValueTask<CallToolResult> AddTimelineTrackAsync(
        [Description(DirectorInstanceIdDescription)]
        int instanceId,
        [Description(
            "Fully qualified track type, e.g. UnityEngine.Timeline.AnimationTrack " +
            "(also AudioTrack, ActivationTrack, ControlTrack, SignalTrack, GroupTrack in the same namespace).")]
        string trackType,
        [Description("Optional track name.")]
        string trackName = "",
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.AddTrackAsync(instanceId, trackType, trackName, ct), cancellationToken);

    [McpServerTool(Name = "remove_timeline_track", ReadOnly = false),
     Description("Remove a track from the TimelineAsset of a PlayableDirector by index."),
     UsedImplicitly]
    public ValueTask<CallToolResult> RemoveTimelineTrackAsync(
        [Description(DirectorInstanceIdDescription)]
        int instanceId,
        [Description(TrackIndexDescription)]
        int trackIndex,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.RemoveTrackAsync(instanceId, trackIndex, ct), cancellationToken);

    [McpServerTool(Name = "bind_timeline_track", ReadOnly = false),
     Description("Set the binding of a Timeline track on a PlayableDirector."),
     UsedImplicitly]
    public ValueTask<CallToolResult> BindTimelineTrackAsync(
        [Description(DirectorInstanceIdDescription)]
        int instanceId,
        [Description(TrackIndexDescription)]
        int trackIndex,
        [Description("instanceId of the object to bind to the track.")]
        int targetInstanceId,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.BindTrackAsync(instanceId, trackIndex, targetInstanceId, ct),
            cancellationToken);

    [McpServerTool(Name = "add_timeline_clip", ReadOnly = false),
     Description("Add a default clip of the track's clip type to a Timeline track."),
     UsedImplicitly]
    public ValueTask<CallToolResult> AddTimelineClipAsync(
        [Description(DirectorInstanceIdDescription)]
        int instanceId,
        [Description(TrackIndexDescription)]
        int trackIndex,
        [Description("Start time in seconds.")]
        double start = 0,
        [Description("Duration in seconds; 0 uses the track's default.")]
        double duration = 0,
        [Description("Optional clip display name.")]
        string clipName = "",
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.AddClipAsync(instanceId, trackIndex, start, duration, clipName, ct),
            cancellationToken);

    [McpServerTool(Name = "remove_timeline_clip", ReadOnly = false),
     Description("Remove a clip from a Timeline track by index."),
     UsedImplicitly]
    public ValueTask<CallToolResult> RemoveTimelineClipAsync(
        [Description(DirectorInstanceIdDescription)]
        int instanceId,
        [Description(TrackIndexDescription)]
        int trackIndex,
        [Description(ClipIndexDescription)]
        int clipIndex,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.RemoveClipAsync(instanceId, trackIndex, clipIndex, ct),
            cancellationToken);

    [McpServerTool(Name = "get_timeline_tracks", ReadOnly = true),
     Description(
         "Get the tracks and clips of a Timeline: track type, group, binding and muted state, and each clip's " +
         "start, duration, timeScale, clipIn, ease, extrapolation and AnimationClip. The indices are the " +
         "trackIndex/clipIndex of the other Timeline tools. Specify instanceId or assetPath; bindings are " +
         "resolved only with instanceId."),
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
         "Get the top-level serialized properties of a Timeline track (e.g. m_TrackOffset), " +
         "for set_timeline_track_property. Hidden ones such as m_Muted are not listed but can be set. " +
         "Specify instanceId or assetPath."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetTimelineTrackPropertiesAsync(
        [Description(TrackIndexDescription)]
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
         "Get the top-level serialized properties of a Timeline clip's PlayableAsset " +
         "(e.g. m_Clip of an Animation clip), for set_timeline_clip_property. " +
         "Specify instanceId or assetPath; ExposedReference values are resolved only with instanceId."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetTimelineClipPropertiesAsync(
        [Description(TrackIndexDescription)]
        int trackIndex,
        [Description(ClipIndexDescription)]
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
         "Change the timing and settings of a Timeline clip; only the given values change. " +
         "timeScale, clipIn, ease and extrapolation fail if the clip type does not support them."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ModifyTimelineClipAsync(
        [Description(DirectorInstanceIdDescription)]
        int instanceId,
        [Description(TrackIndexDescription)]
        int trackIndex,
        [Description(ClipIndexDescription)]
        int clipIndex,
        [Description("Start time in seconds (>= 0).")]
        double? start = null,
        [Description("Duration in seconds (> 0).")]
        double? duration = null,
        [Description("Speed multiplier (> 0).")]
        double? timeScale = null,
        [Description("Offset in seconds into the clip's source where playback starts (>= 0).")]
        double? clipIn = null,
        [Description("Ease in duration in seconds (>= 0).")]
        double? easeInDuration = null,
        [Description("Ease out duration in seconds (>= 0).")]
        double? easeOutDuration = null,
        [Description("Extrapolation before the clip: " + ExtrapolationDescription + ".")]
        string? preExtrapolation = null,
        [Description("Extrapolation after the clip: " + ExtrapolationDescription + ".")]
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
         "Set a serialized property of a Timeline clip's PlayableAsset, e.g. m_Clip to assign an AnimationClip. " +
         "Paths are listed by get_timeline_clip_properties; the clip's timing is changed with modify_timeline_clip. " +
         "ExposedReferences are resolved through the PlayableDirector."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetTimelineClipPropertyAsync(
        [Description(DirectorInstanceIdDescription)]
        int instanceId,
        [Description(TrackIndexDescription)]
        int trackIndex,
        [Description(ClipIndexDescription)]
        int clipIndex,
        [Description("Serialized property path (e.g. \"m_Clip\", \"postPlayback\", \"template.speed\").")]
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
         "Set a serialized property of a Timeline track (e.g. m_TrackOffset, or m_Muted to mute it). " +
         "Paths are listed by get_timeline_track_properties."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetTimelineTrackPropertyAsync(
        [Description(DirectorInstanceIdDescription)]
        int instanceId,
        [Description(TrackIndexDescription)]
        int trackIndex,
        [Description("Serialized property path (e.g. \"m_Muted\", \"m_TrackOffset\", \"m_Position\").")]
        string propertyPath,
        [Description(PropertyValueDescription)]
        string value,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.SetTrackPropertyAsync(instanceId, trackIndex, propertyPath, value, ct),
            cancellationToken);

    [McpServerTool(Name = "play_timeline", ReadOnly = false),
     Description("Start playback of the Timeline on a PlayableDirector."),
     UsedImplicitly]
    public ValueTask<CallToolResult> PlayTimelineAsync(
        [Description(DirectorInstanceIdDescription)]
        int instanceId,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.PlayAsync(instanceId, ct), cancellationToken);

    [McpServerTool(Name = "stop_timeline", ReadOnly = false),
     Description("Stop the Timeline on a PlayableDirector and reset it to the beginning."),
     UsedImplicitly]
    public ValueTask<CallToolResult> StopTimelineAsync(
        [Description(DirectorInstanceIdDescription)]
        int instanceId,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.StopAsync(instanceId, ct), cancellationToken);

    [McpServerTool(Name = "evaluate_timeline", ReadOnly = false),
     Description(
         "Evaluate a Timeline at a time without playing it, so the scene shows that moment. " +
         "In Edit Mode (and Prefab Mode) the Timeline window preview is used, so values revert when the preview ends; " +
         "in Play Mode PlayableDirector.time is set and Evaluate() is called."),
     UsedImplicitly]
    public ValueTask<CallToolResult> EvaluateTimelineAsync(
        [Description(DirectorInstanceIdDescription)]
        int instanceId,
        [Description("Time in seconds (>= 0).")]
        double time,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => timelineUseCase.EvaluateAsync(instanceId, time, ct), cancellationToken);
}
