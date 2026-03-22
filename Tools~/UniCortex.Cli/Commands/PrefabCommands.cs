using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class PrefabCommands(PrefabUseCase prefabService)
{
    /// <summary>Create a Prefab asset from a GameObject in the scene.</summary>
    /// <param name="instanceId">Instance ID of the source GameObject.</param>
    /// <param name="assetPath">Asset path to save the Prefab (e.g. "Assets/Prefabs/MyCube.prefab").</param>
    [Command("create")]
    public async Task Create([Argument] int instanceId, [Argument] string assetPath,
        CancellationToken cancellationToken = default)
    {
        var message = await prefabService.CreateAsync(instanceId, assetPath, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Instantiate a Prefab into the current scene.</summary>
    /// <param name="assetPath">Asset path of the Prefab to instantiate (e.g. "Assets/Prefabs/MyCube.prefab").</param>
    [Command("instantiate")]
    public async Task Instantiate([Argument] string assetPath, CancellationToken cancellationToken = default)
    {
        var json = await prefabService.InstantiateAsync(assetPath, cancellationToken);
        Console.WriteLine(json);
    }
}
