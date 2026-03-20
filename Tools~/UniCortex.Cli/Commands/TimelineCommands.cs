using ConsoleAppFramework;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Commands;

#pragma warning disable CS1573 // Parameter has no matching param tag
public class TimelineCommands(TimelineUseCase timelineService)
{
    /// <summary>Get Timeline information from a PlayableDirector. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    [Command("get-info")]
    public async Task GetInfo(int instanceId, CancellationToken cancellationToken = default)
    {
        var json = await timelineService.GetInfoAsync(instanceId, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Add a track to a TimelineAsset. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackType">Track type: AnimationTrack, AudioTrack, ActivationTrack, ControlTrack, SignalTrack, GroupTrack.</param>
    /// <param name="trackName">Optional name for the new track.</param>
    [Command("add-track")]
    public async Task AddTrack(int instanceId, string trackType, string trackName = "",
        CancellationToken cancellationToken = default)
    {
        var message = await timelineService.AddTrackAsync(instanceId, trackType, trackName, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Remove a track from a TimelineAsset by index. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackIndex">The index of the track to remove (0-based).</param>
    [Command("remove-track")]
    public async Task RemoveTrack(int instanceId, int trackIndex,
        CancellationToken cancellationToken = default)
    {
        var message = await timelineService.RemoveTrackAsync(instanceId, trackIndex, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Set the binding of a Timeline track. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackIndex">The index of the track to bind (0-based).</param>
    /// <param name="targetInstanceId">The instanceId of the target object to bind to the track.</param>
    [Command("set-binding")]
    public async Task SetBinding(int instanceId, int trackIndex, int targetInstanceId,
        CancellationToken cancellationToken = default)
    {
        var message = await timelineService.SetBindingAsync(instanceId, trackIndex, targetInstanceId,
            cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Add a default clip to a Timeline track. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackIndex">The index of the track to add the clip to (0-based).</param>
    /// <param name="start">Start time of the clip in seconds.</param>
    /// <param name="duration">Duration of the clip in seconds. 0 uses the track's default duration.</param>
    /// <param name="clipName">Optional display name for the clip.</param>
    [Command("add-clip")]
    public async Task AddClip(int instanceId, int trackIndex, double start = 0, double duration = 0,
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
    [Command("remove-clip")]
    public async Task RemoveClip(int instanceId, int trackIndex, int clipIndex,
        CancellationToken cancellationToken = default)
    {
        var message = await timelineService.RemoveClipAsync(instanceId, trackIndex, clipIndex, cancellationToken);
        Console.WriteLine(message);
    }
}
