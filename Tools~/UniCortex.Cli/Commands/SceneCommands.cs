using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class SceneCommands(SceneUseCase sceneService)
{
    /// <summary>Create a new empty scene and save it at the specified asset path.</summary>
    [Command("create")]
    public async Task Create(string scenePath, CancellationToken cancellationToken)
    {
        var message = await sceneService.CreateAsync(scenePath, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Open a scene in the Unity Editor by its asset path.</summary>
    [Command("open")]
    public async Task Open(string scenePath, CancellationToken cancellationToken)
    {
        var message = await sceneService.OpenAsync(scenePath, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Save all open scenes in the Unity Editor.</summary>
    [Command("save")]
    public async Task Save(CancellationToken cancellationToken)
    {
        var message = await sceneService.SaveAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Get the GameObject hierarchy of the current scene.</summary>
    [Command("hierarchy")]
    public async Task Hierarchy(CancellationToken cancellationToken)
    {
        var json = await sceneService.GetHierarchyAsync(cancellationToken);
        Console.WriteLine(json);
    }
}
