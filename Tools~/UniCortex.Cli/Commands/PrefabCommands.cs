using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class PrefabCommands(PrefabUseCase prefabUseCase)
{
    /// <summary>Create a Prefab asset from a GameObject in a scene or in Prefab Mode.</summary>
    /// <param name="instanceId">Instance ID of the source GameObject.</param>
    /// <param name="assetPath">Asset path to save the Prefab (e.g. "Assets/Prefabs/MyCube.prefab").</param>
    /// <param name="connect">Turn the GameObject into an instance of the new Prefab (references to it stay valid).</param>
    [Command("create")]
    public async Task Create([Argument] int instanceId, [Argument] string assetPath, bool connect = false,
        CancellationToken cancellationToken = default)
    {
        var message = await prefabUseCase.CreateAsync(instanceId, assetPath, connect, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Instantiate a Prefab into the current scene.</summary>
    /// <param name="assetPath">Asset path of the Prefab to instantiate (e.g. "Assets/Prefabs/MyCube.prefab").</param>
    [Command("instantiate")]
    public async Task Instantiate([Argument] string assetPath, CancellationToken cancellationToken = default)
    {
        var json = await prefabUseCase.InstantiateAsync(assetPath, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Open a Prefab asset in Prefab Mode for editing. Fails while the Prefab already open in Prefab Mode has unsaved changes.</summary>
    /// <param name="assetPath">Asset path of the Prefab to open (e.g. "Assets/Prefabs/MyCube.prefab").</param>
    [Command("open")]
    public async Task Open([Argument] string assetPath, CancellationToken cancellationToken = default)
    {
        var message = await prefabUseCase.OpenAsync(assetPath, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Close Prefab Mode and return to the main stage. Fails while the Prefab has unsaved changes, unless --discard-changes is given.</summary>
    /// <param name="discardChanges">Discard the unsaved changes of the Prefab instead of failing. Cannot be undone.</param>
    [Command("close")]
    public async Task Close(bool discardChanges = false, CancellationToken cancellationToken = default)
    {
        var message = await prefabUseCase.CloseAsync(discardChanges, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Unpack a prefab instance (in a scene or in Prefab Mode) into plain GameObjects. Undoable.</summary>
    /// <param name="instanceId">Instance ID of the outermost root of the prefab instance.</param>
    /// <param name="mode">OutermostRoot (nested prefab instances inside stay linked) or Completely (unpack them too).</param>
    [Command("unpack")]
    public async Task Unpack([Argument] int instanceId, string mode = "OutermostRoot",
        CancellationToken cancellationToken = default)
    {
        var message = await prefabUseCase.UnpackAsync(instanceId, mode, cancellationToken);
        Console.WriteLine(message);
    }
}
