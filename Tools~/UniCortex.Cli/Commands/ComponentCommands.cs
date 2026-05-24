using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ComponentCommands(ComponentUseCase componentUseCase)
{
    /// <summary>Add a component to a GameObject.</summary>
    /// <param name="instanceId">Instance ID of the target GameObject.</param>
    /// <param name="componentType">Fully-qualified component type name (e.g. "UnityEngine.Rigidbody").</param>
    /// <param name="assemblyName">Name of the assembly that defines the type (e.g. "UnityEngine.PhysicsModule").</param>
    [Command("add")]
    public async Task Add([Argument] int instanceId, [Argument] string componentType,
        [Argument] string assemblyName, CancellationToken cancellationToken = default)
    {
        var message = await componentUseCase.AddAsync(instanceId, componentType, assemblyName, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Remove a component from a GameObject.</summary>
    /// <param name="instanceId">Instance ID of the target GameObject.</param>
    /// <param name="componentType">Fully-qualified component type name (e.g. "UnityEngine.Rigidbody").</param>
    /// <param name="assemblyName">Name of the assembly that defines the type (e.g. "UnityEngine.PhysicsModule").</param>
    /// <param name="componentIndex">Index when multiple components of the same type exist.</param>
    [Command("remove")]
    public async Task Remove([Argument] int instanceId, [Argument] string componentType,
        [Argument] string assemblyName, int componentIndex = 0, CancellationToken cancellationToken = default)
    {
        var message = await componentUseCase.RemoveAsync(instanceId, componentType, assemblyName, componentIndex,
            cancellationToken);
        Console.WriteLine(message);
    }
}
