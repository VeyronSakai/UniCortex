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
    [McpServerTool(Name = "get_timeline_info", ReadOnly = true),
     Description(
         "Get Timeline information from a PlayableDirector including tracks, clips, bindings, and playback state. " +
         "Requires the Timeline package (com.unity.timeline) to be installed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> GetTimelineInfoAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component, or the PlayableDirector component itself.")]
        int instanceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await timelineService.GetInfoAsync(instanceId, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = json }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "set_timeline_time", ReadOnly = false),
     Description(
         "Set the current playback time of a PlayableDirector's Timeline. " +
         "Requires the Timeline package (com.unity.timeline) to be installed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SetTimelineTimeAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description("The time in seconds to set the playback position to.")]
        double time,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await timelineService.SetTimeAsync(instanceId, time, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "play_timeline", ReadOnly = false),
     Description(
         "Play a Timeline on a PlayableDirector. " +
         "Requires the Timeline package (com.unity.timeline) to be installed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> PlayTimelineAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await timelineService.PlayAsync(instanceId, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "pause_timeline", ReadOnly = false),
     Description(
         "Pause a playing Timeline on a PlayableDirector. " +
         "Requires the Timeline package (com.unity.timeline) to be installed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> PauseTimelineAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await timelineService.PauseAsync(instanceId, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "stop_timeline", ReadOnly = false),
     Description(
         "Stop a playing Timeline on a PlayableDirector. " +
         "Requires the Timeline package (com.unity.timeline) to be installed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> StopTimelineAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await timelineService.StopAsync(instanceId, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "add_timeline_track", ReadOnly = false),
     Description(
         "Add a track to a TimelineAsset on a PlayableDirector. Undo supported. " +
         "Requires the Timeline package (com.unity.timeline) to be installed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> AddTimelineTrackAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description(
            "Track type to add: " +
            TimelineTrackType.AnimationTrack + ", " + TimelineTrackType.AudioTrack + ", " +
            TimelineTrackType.ActivationTrack + ", " + TimelineTrackType.ControlTrack + ", " +
            TimelineTrackType.SignalTrack + ", " + TimelineTrackType.GroupTrack + ".")]
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
         "Remove a track from a TimelineAsset on a PlayableDirector by index. Undo supported. " +
         "Requires the Timeline package (com.unity.timeline) to be installed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> RemoveTimelineTrackAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description("The index of the track to remove (0-based, from get_timeline_info output).")]
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

    [McpServerTool(Name = "set_timeline_binding", ReadOnly = false),
     Description(
         "Set the binding of a Timeline track on a PlayableDirector. Undo supported. " +
         "Requires the Timeline package (com.unity.timeline) to be installed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SetTimelineBindingAsync(
        [Description("The instanceId of a GameObject with a PlayableDirector component.")]
        int instanceId,
        [Description("The index of the track to bind (0-based, from get_timeline_info output).")]
        int trackIndex,
        [Description("The instanceId of the target object to bind to the track.")]
        int targetInstanceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await timelineService.SetBindingAsync(instanceId, trackIndex, targetInstanceId,
                cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
