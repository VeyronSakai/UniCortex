using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class GameObjectCommands(GameObjectUseCase gameObjectService)
{
    /// <summary>Find GameObjects in the current scene.</summary>
    /// <param name="query">Search query string using Unity Search syntax (e.g. "t:Camera", "tag:Player"). Returns all GameObjects if omitted.</param>
    [Command("find")]
    public async Task Find(string? query = null, CancellationToken cancellationToken = default)
    {
        var json = await gameObjectService.FindAsync(query, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Create a new empty GameObject in the current scene.</summary>
    /// <param name="name">Name of the GameObject to create.</param>
    [Command("create")]
    public async Task Create([Argument] string name, CancellationToken cancellationToken = default)
    {
        var json = await gameObjectService.CreateAsync(name, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Delete a GameObject from the current scene by its instance ID.</summary>
    /// <param name="instanceId">Instance ID of the GameObject to delete.</param>
    [Command("delete")]
    public async Task Delete([Argument] int instanceId, CancellationToken cancellationToken = default)
    {
        var message = await gameObjectService.DeleteAsync(instanceId, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Modify a GameObject's properties.</summary>
    /// <param name="instanceId">Instance ID of the GameObject to modify.</param>
    /// <param name="name">New name for the GameObject.</param>
    /// <param name="activeSelf">Set active state of the GameObject.</param>
    /// <param name="tag">Tag to assign to the GameObject.</param>
    /// <param name="layer">Layer number to assign to the GameObject.</param>
    /// <param name="parentInstanceId">Instance ID of the new parent GameObject. Use 0 to move to root.</param>
    [Command("modify")]
    public async Task Modify([Argument] int instanceId, string? name = null, bool? activeSelf = null,
        string? tag = null, int? layer = null, int? parentInstanceId = null,
        CancellationToken cancellationToken = default)
    {
        var message = await gameObjectService.ModifyAsync(instanceId, name, activeSelf, tag, layer,
            parentInstanceId, cancellationToken);
        Console.WriteLine(message);
    }
}
