using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ComponentCommands(ComponentUseCase componentService)
{
    /// <summary>Add a component to a GameObject.</summary>
    /// <param name="instanceId">Instance ID of the target GameObject.</param>
    /// <param name="componentType">Fully qualified component type name (e.g. "UnityEngine.Rigidbody").</param>
    [Command("add")]
    public async Task Add(int instanceId, string componentType,
        CancellationToken cancellationToken = default)
    {
        var message = await componentService.AddAsync(instanceId, componentType, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Remove a component from a GameObject.</summary>
    /// <param name="instanceId">Instance ID of the target GameObject.</param>
    /// <param name="componentType">Fully qualified component type name (e.g. "UnityEngine.Rigidbody").</param>
    /// <param name="componentIndex">Index when multiple components of the same type exist.</param>
    [Command("remove")]
    public async Task Remove(int instanceId, string componentType, int componentIndex = 0,
        CancellationToken cancellationToken = default)
    {
        var message = await componentService.RemoveAsync(instanceId, componentType, componentIndex,
            cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Get serialized properties of a component on a GameObject.</summary>
    /// <param name="instanceId">Instance ID of the target GameObject.</param>
    /// <param name="componentType">Fully qualified component type name (e.g. "UnityEngine.Transform").</param>
    /// <param name="componentIndex">Index when multiple components of the same type exist.</param>
    [Command("properties")]
    public async Task Properties(int instanceId, string componentType, int componentIndex = 0,
        CancellationToken cancellationToken = default)
    {
        var json = await componentService.GetPropertiesAsync(instanceId, componentType, componentIndex,
            cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Set a serialized property on a component.</summary>
    /// <param name="instanceId">Instance ID of the target GameObject.</param>
    /// <param name="componentType">Fully qualified component type name (e.g. "UnityEngine.Transform").</param>
    /// <param name="propertyPath">Serialized property path (e.g. "m_LocalPosition.x").</param>
    /// <param name="value">Value to set as a string. Type is auto-detected from the property.</param>
    [Command("set-property")]
    public async Task SetProperty(int instanceId, string componentType, string propertyPath,
        string value, CancellationToken cancellationToken = default)
    {
        var message = await componentService.SetPropertyAsync(instanceId, componentType, propertyPath,
            value, cancellationToken);
        Console.WriteLine(message);
    }
}
