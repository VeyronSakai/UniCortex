using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

#pragma warning disable CS1573 // Parameter has no matching param tag
public class TimelineBindingCommands(TimelineUseCase timelineService)
{
    /// <summary>Set the binding of a Timeline track. Undo supported. Requires com.unity.timeline.</summary>
    /// <param name="instanceId">The instanceId of a GameObject with a PlayableDirector component.</param>
    /// <param name="trackIndex">The index of the track to bind (0-based).</param>
    /// <param name="targetInstanceId">The instanceId of the target object to bind to the track.</param>
    [Command("set")]
    public async Task Set(int instanceId, int trackIndex, int targetInstanceId,
        CancellationToken cancellationToken = default)
    {
        var message = await timelineService.SetBindingAsync(instanceId, trackIndex, targetInstanceId,
            cancellationToken);
        Console.WriteLine(message);
    }
}
