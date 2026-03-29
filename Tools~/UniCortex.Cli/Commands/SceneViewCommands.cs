using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class SceneViewCommands(SceneViewUseCase sceneViewUseCase)
{
    /// <summary>Switch focus to the Scene View window.</summary>
    [Command("focus")]
    public async Task Focus(CancellationToken cancellationToken = default)
    {
        var message = await sceneViewUseCase.FocusAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
