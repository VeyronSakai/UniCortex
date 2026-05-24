using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ComponentPropertyCommands(ComponentUseCase componentUseCase)
{
    /// <summary>List serialized properties of a component on a GameObject.</summary>
    /// <param name="instanceId">Instance ID of the target GameObject.</param>
    /// <param name="componentType">Fully qualified component type name (e.g. "UnityEngine.Transform").</param>
    /// <param name="componentIndex">Index when multiple components of the same type exist.</param>
    [Command("list")]
    public async Task List([Argument] int instanceId, [Argument] string componentType, int componentIndex = 0,
        CancellationToken cancellationToken = default)
    {
        var json = await componentUseCase.GetPropertiesAsync(instanceId, componentType, componentIndex,
            cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Set a serialized property on a component.</summary>
    /// <param name="instanceId">Instance ID of the target GameObject.</param>
    /// <param name="componentType">Fully qualified component type name (e.g. "UnityEngine.Transform").</param>
    /// <param name="propertyPath">Serialized property path (e.g. "m_LocalPosition.x").</param>
    /// <param name="value">Value to set as a string. Type is auto-detected from the property.</param>
    [Command("set")]
    public async Task Set([Argument] int instanceId, [Argument] string componentType,
        [Argument] string propertyPath, [Argument] string value,
        CancellationToken cancellationToken = default)
    {
        var message = await componentUseCase.SetPropertyAsync(instanceId, componentType, propertyPath,
            value, cancellationToken);
        Console.WriteLine(message);
    }
}
