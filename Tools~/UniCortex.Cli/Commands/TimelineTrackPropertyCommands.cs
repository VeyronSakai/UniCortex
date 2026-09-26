using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

#pragma warning disable CS1573 // Parameter has no matching param tag
public class TimelineTrackPropertyCommands(TimelineUseCase timelineUseCase)
{
    /// <summary>List the serialized properties of a Timeline track as JSON. Specify either instanceId or assetPath. Requires com.unity.timeline.</summary>
    /// <param name="trackIndex">The index of the track (0-based).</param>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component. 0 to use assetPath instead.</param>
    /// <param name="assetPath">Asset path of the TimelineAsset. Used when instanceId is 0.</param>
    [Command("list")]
    public async Task List([Argument] int trackIndex, int instanceId = 0, string? assetPath = null, CancellationToken cancellationToken = default)
    {
        var json = await timelineUseCase.GetTrackPropertiesAsync(instanceId, assetPath, trackIndex, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Set a serialized property on a Timeline track. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackIndex">The index of the track (0-based).</param>
    /// <param name="propertyPath">The serialized property path (e.g. "m_Muted", "m_TrackOffset").</param>
    /// <param name="value">The new value as a string.</param>
    [Command("set")]
    public async Task Set([Argument] int instanceId, [Argument] int trackIndex, [Argument] string propertyPath,
        [Argument] string value, CancellationToken cancellationToken = default)
    {
        var message = await timelineUseCase.SetTrackPropertyAsync(instanceId, trackIndex, propertyPath, value,
            cancellationToken);
        Console.WriteLine(message);
    }
}
