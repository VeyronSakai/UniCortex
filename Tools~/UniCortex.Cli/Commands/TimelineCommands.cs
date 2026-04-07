using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

#pragma warning disable CS1573 // Parameter has no matching param tag
public class TimelineCommands(TimelineUseCase timelineUseCase)
{
    /// <summary>Create a new TimelineAsset (.playable file). Requires com.unity.timeline.</summary>
    /// <param name="assetPath">Asset path where the TimelineAsset will be saved (e.g. "Assets/Timelines/MyTimeline.playable").</param>
    [Command("create")]
    public async Task Create([Argument] string assetPath, CancellationToken cancellationToken = default)
    {
        var json = await timelineUseCase.CreateAsync(assetPath, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Start playback of a Timeline on a PlayableDirector. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    [Command("play")]
    public async Task Play([Argument] int instanceId, CancellationToken cancellationToken = default)
    {
        var message = await timelineUseCase.PlayAsync(instanceId, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Stop playback of a Timeline on a PlayableDirector. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    [Command("stop")]
    public async Task Stop([Argument] int instanceId, CancellationToken cancellationToken = default)
    {
        var message = await timelineUseCase.StopAsync(instanceId, cancellationToken);
        Console.WriteLine(message);
    }
}
