using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

#pragma warning disable CS1573 // Parameter has no matching param tag
public class TimelineTrackCommands(TimelineUseCase timelineService)
{
    /// <summary>Add a track to a TimelineAsset. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackType">Fully qualified type name of the track (e.g. UnityEngine.Timeline.AnimationTrack).</param>
    /// <param name="trackName">Optional name for the new track.</param>
    [Command("add")]
    public async Task Add(int instanceId, string trackType, string trackName = "",
        CancellationToken cancellationToken = default)
    {
        var message = await timelineService.AddTrackAsync(instanceId, trackType, trackName, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Remove a track from a TimelineAsset by index. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackIndex">The index of the track to remove (0-based).</param>
    [Command("remove")]
    public async Task Remove(int instanceId, int trackIndex,
        CancellationToken cancellationToken = default)
    {
        var message = await timelineService.RemoveTrackAsync(instanceId, trackIndex, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Set the binding of a Timeline track. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackIndex">The index of the track to bind (0-based).</param>
    /// <param name="targetInstanceId">The instanceId of the target object to bind to the track.</param>
    [Command("bind")]
    public async Task Bind(int instanceId, int trackIndex, int targetInstanceId,
        CancellationToken cancellationToken = default)
    {
        var message = await timelineService.BindTrackAsync(instanceId, trackIndex, targetInstanceId,
            cancellationToken);
        Console.WriteLine(message);
    }
}
