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
}
