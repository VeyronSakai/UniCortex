using ConsoleAppFramework;
using UniCortex.Core.Services;

namespace UniCortex.Cli.Commands;

public class GameObjectCommands(GameObjectService gameObjectService)
{
    /// <summary>Find GameObjects in the current scene.</summary>
    [Command("find")]
    public async Task Find(string? query = null, CancellationToken cancellationToken = default)
    {
        var json = await gameObjectService.FindAsync(query, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Create a new empty GameObject in the current scene.</summary>
    [Command("create")]
    public async Task Create(string name, CancellationToken cancellationToken = default)
    {
        var json = await gameObjectService.CreateAsync(name, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Delete a GameObject from the current scene by its instance ID.</summary>
    [Command("delete")]
    public async Task Delete(int instanceId, CancellationToken cancellationToken = default)
    {
        var message = await gameObjectService.DeleteAsync(instanceId, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Modify a GameObject's properties.</summary>
    [Command("modify")]
    public async Task Modify(int instanceId, string? name = null, bool? activeSelf = null,
        string? tag = null, int? layer = null, int? parentInstanceId = null,
        CancellationToken cancellationToken = default)
    {
        var message = await gameObjectService.ModifyAsync(instanceId, name, activeSelf, tag, layer,
            parentInstanceId, cancellationToken);
        Console.WriteLine(message);
    }
}
