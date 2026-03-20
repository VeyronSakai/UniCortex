using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class TimelineTools(TimelineUseCase timelineService)
{
    [McpServerTool(Name = "create_timeline", ReadOnly = false),
     Description(
         "Create a new TimelineAsset (.playable file) at the specified asset path."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> CreateTimelineAsync(
        [Description("Asset path where the TimelineAsset will be saved (e.g. \"Assets/Timelines/MyTimeline.playable\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await timelineService.CreateAsync(assetPath, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = json }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "add_timeline_track", ReadOnly = false),
     Description(
         "Add a track to a TimelineAsset on a PlayableDirector. Undo supported."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> AddTimelineTrackAsync(
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
    {
        try
        {
            var message =
                await timelineService.AddTrackAsync(instanceId, trackType, trackName, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "remove_timeline_track", ReadOnly = false),
     Description(
         "Remove a track from a TimelineAsset on a PlayableDirector by index. Undo supported."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> RemoveTimelineTrackAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description("The index of the track to remove (0-based).")]
        int trackIndex,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message =
                await timelineService.RemoveTrackAsync(instanceId, trackIndex, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "bind_timeline_track", ReadOnly = false),
     Description(
         "Set the binding of a Timeline track on a PlayableDirector. Undo supported."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> BindTimelineTrackAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description("The index of the track to bind (0-based).")]
        int trackIndex,
        [Description("The instanceId of the target object to bind to the track.")]
        int targetInstanceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await timelineService.BindTrackAsync(instanceId, trackIndex, targetInstanceId,
                cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "add_timeline_clip", ReadOnly = false),
     Description(
         "Add a default clip to a Timeline track. The clip type is determined by the track type. Undo supported."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> AddTimelineClipAsync(
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
    {
        try
        {
            var message = await timelineService.AddClipAsync(instanceId, trackIndex, start, duration, clipName,
                cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "remove_timeline_clip", ReadOnly = false),
     Description(
         "Remove a clip from a Timeline track by index. Undo supported."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> RemoveTimelineClipAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description("The index of the track containing the clip (0-based).")]
        int trackIndex,
        [Description("The index of the clip to remove within the track (0-based).")]
        int clipIndex,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await timelineService.RemoveClipAsync(instanceId, trackIndex, clipIndex,
                cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
