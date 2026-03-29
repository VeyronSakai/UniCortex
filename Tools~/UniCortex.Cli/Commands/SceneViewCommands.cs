using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class SceneViewCommands(ViewUseCase viewUseCase)
{
    /// <summary>Switch focus to the Scene View window.</summary>
    [Command("focus")]
    public async Task Focus(CancellationToken cancellationToken = default)
    {
        var message = await viewUseCase.FocusSceneViewAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
