using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class GameObjectCommands(GameObjectUseCase gameObjectUseCase)
{
    /// <summary>Find GameObjects in the current scene.</summary>
    /// <param name="query">Search query string using Unity Search syntax (e.g. "t:Camera", "tag:Player").</param>
    [Command("find")]
    public async Task Find([Argument] string query, CancellationToken cancellationToken = default)
    {
        var json = await gameObjectUseCase.FindAsync(query, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Create a new empty GameObject in the current scene.</summary>
    /// <param name="name">Name of the GameObject to create.</param>
    /// <param name="parentInstanceId">Instance ID of the parent GameObject. If omitted, created at the scene root.</param>
    /// <param name="siblingIndex">Position among siblings (0 = first). If omitted, placed last.</param>
    /// <param name="useRectTransform">Create with a RectTransform for UI. Automatic when the parent has a RectTransform.</param>
    [Command("create")]
    public async Task Create([Argument] string name, int? parentInstanceId = null, int? siblingIndex = null,
        bool useRectTransform = false, CancellationToken cancellationToken = default)
    {
        var json = await gameObjectUseCase.CreateAsync(name, parentInstanceId, siblingIndex, useRectTransform,
            cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Delete a GameObject from the current scene by its instance ID.</summary>
    /// <param name="instanceId">Instance ID of the GameObject to delete.</param>
    [Command("delete")]
    public async Task Delete([Argument] int instanceId, CancellationToken cancellationToken = default)
    {
        var message = await gameObjectUseCase.DeleteAsync(instanceId, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Modify a GameObject's properties.</summary>
    /// <param name="instanceId">Instance ID of the GameObject to modify.</param>
    /// <param name="name">New name for the GameObject.</param>
    /// <param name="activate">Set the GameObject active. Cannot be combined with --deactivate.</param>
    /// <param name="deactivate">Set the GameObject inactive. Cannot be combined with --activate.</param>
    /// <param name="tag">Tag to assign to the GameObject.</param>
    /// <param name="layer">Layer number to assign to the GameObject.</param>
    /// <param name="parentInstanceId">Instance ID of the new parent GameObject. Use 0 to move to root.</param>
    /// <param name="siblingIndex">New position among siblings (0 = first).</param>
    /// <param name="keepLocalTransform">When changing the parent, keep the local transform instead of the world transform.</param>
    [Command("modify")]
    public async Task Modify([Argument] int instanceId, string? name = null, bool activate = false,
        bool deactivate = false, string? tag = null, int? layer = null, int? parentInstanceId = null,
        int? siblingIndex = null, bool keepLocalTransform = false, CancellationToken cancellationToken = default)
    {
        if (activate && deactivate)
        {
            throw new ArgumentException("--activate and --deactivate cannot be used together.");
        }

        // Neither flag leaves the active state unchanged.
        bool? activeSelf = activate ? true : deactivate ? false : null;
        var message = await gameObjectUseCase.ModifyAsync(instanceId, name, activeSelf, tag, layer,
            parentInstanceId, siblingIndex, keepLocalTransform ? false : null, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Duplicate a GameObject, deep-copying its children and components.</summary>
    /// <param name="instanceId">Instance ID of the GameObject to duplicate.</param>
    /// <param name="name">Optional name for the duplicate. If omitted, a Unity-style unique name like "Foo (1)" is assigned.</param>
    [Command("duplicate")]
    public async Task Duplicate([Argument] int instanceId, string? name = null,
        CancellationToken cancellationToken = default)
    {
        var json = await gameObjectUseCase.DuplicateAsync(instanceId, name, cancellationToken);
        Console.WriteLine(json);
    }
}
