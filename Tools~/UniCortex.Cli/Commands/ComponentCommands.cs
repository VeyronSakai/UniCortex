using ConsoleAppFramework;
using UniCortex.Core.Services;

namespace UniCortex.Cli.Commands;

public class ComponentCommands(ComponentService componentService)
{
    /// <summary>Add a component to a GameObject.</summary>
    [Command("add")]
    public async Task Add(int instanceId, string componentType,
        CancellationToken cancellationToken = default)
    {
        var message = await componentService.AddAsync(instanceId, componentType, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Remove a component from a GameObject.</summary>
    [Command("remove")]
    public async Task Remove(int instanceId, string componentType, int componentIndex = 0,
        CancellationToken cancellationToken = default)
    {
        var message = await componentService.RemoveAsync(instanceId, componentType, componentIndex,
            cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Get serialized properties of a component on a GameObject.</summary>
    [Command("properties")]
    public async Task Properties(int instanceId, string componentType, int componentIndex = 0,
        CancellationToken cancellationToken = default)
    {
        var json = await componentService.GetPropertiesAsync(instanceId, componentType, componentIndex,
            cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Set a serialized property on a component.</summary>
    [Command("set-property")]
    public async Task SetProperty(int instanceId, string componentType, string propertyPath,
        string value, CancellationToken cancellationToken = default)
    {
        var message = await componentService.SetPropertyAsync(instanceId, componentType, propertyPath,
            value, cancellationToken);
        Console.WriteLine(message);
    }
}
