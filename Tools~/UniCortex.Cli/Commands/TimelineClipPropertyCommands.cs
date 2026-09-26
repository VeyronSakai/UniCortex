using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

#pragma warning disable CS1573 // Parameter has no matching param tag
public class TimelineClipPropertyCommands(TimelineUseCase timelineUseCase)
{
    /// <summary>List the serialized properties of the content (PlayableAsset) of a Timeline clip as JSON. Specify either instanceId or assetPath. Requires com.unity.timeline.</summary>
    /// <param name="trackIndex">The index of the track containing the clip (0-based).</param>
    /// <param name="clipIndex">The index of the clip within the track (0-based).</param>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component. 0 to use assetPath instead.</param>
    /// <param name="assetPath">Asset path of the TimelineAsset. Used when instanceId is 0.</param>
    [Command("list")]
    public async Task List([Argument] int trackIndex, [Argument] int clipIndex, int instanceId = 0,
        string? assetPath = null, CancellationToken cancellationToken = default)
    {
        var json = await timelineUseCase.GetClipPropertiesAsync(instanceId, assetPath, trackIndex, clipIndex, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Set a serialized property on the content (PlayableAsset) of a Timeline clip, e.g. m_Clip to assign an AnimationClip. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackIndex">The index of the track containing the clip (0-based).</param>
    /// <param name="clipIndex">The index of the clip within the track (0-based).</param>
    /// <param name="propertyPath">The serialized property path (e.g. "m_Clip", "postPlayback").</param>
    /// <param name="value">The new value as a string.</param>
    [Command("set")]
    public async Task Set([Argument] int instanceId, [Argument] int trackIndex, [Argument] int clipIndex,
        [Argument] string propertyPath, [Argument] string value, CancellationToken cancellationToken = default)
    {
        var message = await timelineUseCase.SetClipPropertyAsync(instanceId, trackIndex, clipIndex,
            propertyPath, value, cancellationToken);
        Console.WriteLine(message);
    }
}
