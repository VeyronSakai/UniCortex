using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class SceneCommands(SceneUseCase sceneUseCase)
{
    /// <summary>Create a new empty scene and save it at the specified asset path.</summary>
    /// <param name="scenePath">Asset path to save the scene (e.g. "Assets/Scenes/NewScene.unity").</param>
    [Command("create")]
    public async Task Create([Argument] string scenePath, CancellationToken cancellationToken)
    {
        var message = await sceneUseCase.CreateAsync(scenePath, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Open a scene in the Unity Editor by its asset path.</summary>
    /// <param name="scenePath">Asset path of the scene to open (e.g. "Assets/Scenes/Main.unity").</param>
    [Command("open")]
    public async Task Open([Argument] string scenePath, CancellationToken cancellationToken)
    {
        var message = await sceneUseCase.OpenAsync(scenePath, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Save all open scenes in the Unity Editor.</summary>
    [Command("save")]
    public async Task Save(CancellationToken cancellationToken)
    {
        var message = await sceneUseCase.SaveAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Get the GameObject hierarchy of the current scene.</summary>
    [Command("hierarchy")]
    public async Task Hierarchy(CancellationToken cancellationToken)
    {
        var json = await sceneUseCase.GetHierarchyAsync(cancellationToken);
        Console.WriteLine(json);
    }
}
