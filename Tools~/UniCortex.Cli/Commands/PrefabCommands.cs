using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class PrefabCommands(PrefabUseCase prefabUseCase)
{
    /// <summary>Create a Prefab asset from a GameObject in the scene.</summary>
    /// <param name="instanceId">Instance ID of the source GameObject.</param>
    /// <param name="assetPath">Asset path to save the Prefab (e.g. "Assets/Prefabs/MyCube.prefab").</param>
    [Command("create")]
    public async Task Create([Argument] int instanceId, [Argument] string assetPath,
        CancellationToken cancellationToken = default)
    {
        var message = await prefabUseCase.CreateAsync(instanceId, assetPath, cancellationToken);
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

    /// <summary>Open a Prefab asset in Prefab Mode for editing.</summary>
    /// <param name="assetPath">Asset path of the Prefab to open (e.g. "Assets/Prefabs/MyCube.prefab").</param>
    [Command("open")]
    public async Task Open([Argument] string assetPath, CancellationToken cancellationToken = default)
    {
        var message = await prefabUseCase.OpenAsync(assetPath, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Close Prefab Mode and return to the main stage.</summary>
    [Command("close")]
    public async Task Close(CancellationToken cancellationToken = default)
    {
        var message = await prefabUseCase.CloseAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
