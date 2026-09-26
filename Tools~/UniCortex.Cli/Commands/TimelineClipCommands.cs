using ConsoleAppFramework;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Commands;

#pragma warning disable CS1573 // Parameter has no matching param tag
public class TimelineClipCommands(TimelineUseCase timelineUseCase)
{
    /// <summary>Add a default clip to a Timeline track. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackIndex">The index of the track to add the clip to (0-based).</param>
    /// <param name="start">Start time of the clip in seconds.</param>
    /// <param name="duration">Duration of the clip in seconds. 0 uses the track's default duration.</param>
    /// <param name="clipName">Optional display name for the clip.</param>
    [Command("add")]
    public async Task Add([Argument] int instanceId, [Argument] int trackIndex, double start = 0, double duration = 0,
        string clipName = "", CancellationToken cancellationToken = default)
    {
        var message = await timelineUseCase.AddClipAsync(instanceId, trackIndex, start, duration, clipName,
            cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Remove a clip from a Timeline track by index. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackIndex">The index of the track containing the clip (0-based).</param>
    /// <param name="clipIndex">The index of the clip to remove within the track (0-based).</param>
    [Command("remove")]
    public async Task Remove([Argument] int instanceId, [Argument] int trackIndex, [Argument] int clipIndex,
        CancellationToken cancellationToken = default)
    {
        var message = await timelineUseCase.RemoveClipAsync(instanceId, trackIndex, clipIndex, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Change the timing and settings of a Timeline clip. Only the specified values are changed. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackIndex">The index of the track containing the clip (0-based).</param>
    /// <param name="clipIndex">The index of the clip within the track (0-based).</param>
    /// <param name="start">Start time of the clip in seconds (>= 0).</param>
    /// <param name="duration">Duration of the clip in seconds (> 0).</param>
    /// <param name="timeScale">Speed multiplier of the clip (> 0).</param>
    /// <param name="clipIn">Offset in seconds into the clip's source where playback starts (>= 0).</param>
    /// <param name="easeInDuration">Ease in duration in seconds (>= 0).</param>
    /// <param name="easeOutDuration">Ease out duration in seconds (>= 0).</param>
    /// <param name="preExtrapolation">Extrapolation before the clip (None, Hold, Loop, PingPong, Continue).</param>
    /// <param name="postExtrapolation">Extrapolation after the clip (None, Hold, Loop, PingPong, Continue).</param>
    /// <param name="displayName">Display name of the clip.</param>
    [Command("modify")]
    public async Task Modify([Argument] int instanceId, [Argument] int trackIndex, [Argument] int clipIndex,
        double? start = null, double? duration = null, double? timeScale = null, double? clipIn = null,
        double? easeInDuration = null, double? easeOutDuration = null, string? preExtrapolation = null,
        string? postExtrapolation = null, string? displayName = null, CancellationToken cancellationToken = default)
    {
        var request = new ModifyTimelineClipRequest
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
        };
        var message = await timelineUseCase.ModifyClipAsync(request, cancellationToken);
        Console.WriteLine(message);
    }
}
