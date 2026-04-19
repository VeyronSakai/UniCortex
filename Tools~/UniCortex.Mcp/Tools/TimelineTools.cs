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
