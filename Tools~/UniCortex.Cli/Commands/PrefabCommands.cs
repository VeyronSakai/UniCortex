using ConsoleAppFramework;
using UniCortex.Core.Services;

namespace UniCortex.Cli.Commands;

public class PrefabCommands(PrefabService prefabService)
{
    /// <summary>Create a Prefab asset from a GameObject in the scene.</summary>
    [Command("create")]
    public async Task Create(int instanceId, string assetPath,
        CancellationToken cancellationToken = default)
    {
        var message = await prefabService.CreateAsync(instanceId, assetPath, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Instantiate a Prefab into the current scene.</summary>
    [Command("instantiate")]
    public async Task Instantiate(string assetPath, CancellationToken cancellationToken = default)
    {
        var json = await prefabService.InstantiateAsync(assetPath, cancellationToken);
        Console.WriteLine(json);
    }
}
