using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

#pragma warning disable CS1573 // Parameter has no matching param tag
public class TimelineClipCommands(TimelineUseCase timelineService)
{
    /// <summary>Add a default clip to a Timeline track. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackIndex">The index of the track to add the clip to (0-based).</param>
    /// <param name="start">Start time of the clip in seconds.</param>
    /// <param name="duration">Duration of the clip in seconds. 0 uses the track's default duration.</param>
    /// <param name="clipName">Optional display name for the clip.</param>
    [Command("add")]
    public async Task Add(int instanceId, int trackIndex, double start = 0, double duration = 0,
        string clipName = "", CancellationToken cancellationToken = default)
    {
        var message = await timelineService.AddClipAsync(instanceId, trackIndex, start, duration, clipName,
            cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Remove a clip from a Timeline track by index. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackIndex">The index of the track containing the clip (0-based).</param>
    /// <param name="clipIndex">The index of the clip to remove within the track (0-based).</param>
    [Command("remove")]
    public async Task Remove(int instanceId, int trackIndex, int clipIndex,
        CancellationToken cancellationToken = default)
    {
        var message = await timelineService.RemoveClipAsync(instanceId, trackIndex, clipIndex, cancellationToken);
        Console.WriteLine(message);
    }
}
