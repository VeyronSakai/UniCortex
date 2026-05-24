using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ComponentCommands(ComponentUseCase componentUseCase)
{
    /// <summary>Add a component to a GameObject.</summary>
    /// <param name="instanceId">Instance ID of the target GameObject.</param>
    /// <param name="componentType">Assembly-qualified component type name (e.g. "UnityEngine.Rigidbody, UnityEngine.PhysicsModule").</param>
    [Command("add")]
    public async Task Add([Argument] int instanceId, [Argument] string componentType,
        CancellationToken cancellationToken = default)
    {
        var message = await componentUseCase.AddAsync(instanceId, componentType, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Remove a component from a GameObject.</summary>
    /// <param name="instanceId">Instance ID of the target GameObject.</param>
    /// <param name="componentType">Assembly-qualified component type name (e.g. "UnityEngine.Rigidbody, UnityEngine.PhysicsModule").</param>
    /// <param name="componentIndex">Index when multiple components of the same type exist.</param>
    [Command("remove")]
    public async Task Remove([Argument] int instanceId, [Argument] string componentType, int componentIndex = 0,
        CancellationToken cancellationToken = default)
    {
        var message = await componentUseCase.RemoveAsync(instanceId, componentType, componentIndex,
            cancellationToken);
        Console.WriteLine(message);
    }

}
