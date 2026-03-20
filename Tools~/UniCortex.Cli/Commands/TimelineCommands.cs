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

    /// <summary>Set the current playback time of a Timeline. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="time">The time in seconds to set the playback position to.</param>
    [Command("set-time")]
    public async Task SetTime(int instanceId, double time, CancellationToken cancellationToken = default)
    {
        var message = await timelineService.SetTimeAsync(instanceId, time, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Play a Timeline. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    [Command("play")]
    public async Task Play(int instanceId, CancellationToken cancellationToken = default)
    {
        var message = await timelineService.PlayAsync(instanceId, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Pause a playing Timeline. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    [Command("pause")]
    public async Task Pause(int instanceId, CancellationToken cancellationToken = default)
    {
        var message = await timelineService.PauseAsync(instanceId, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Stop a playing Timeline. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    [Command("stop")]
    public async Task Stop(int instanceId, CancellationToken cancellationToken = default)
    {
        var message = await timelineService.StopAsync(instanceId, cancellationToken);
        Console.WriteLine(message);
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
}
