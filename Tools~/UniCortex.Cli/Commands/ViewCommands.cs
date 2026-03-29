using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ViewCommands(ViewUseCase viewUseCase)
{
    /// <summary>Switch focus to the Scene View window.</summary>
    [Command("focus-scene")]
    public async Task FocusScene(CancellationToken cancellationToken = default)
    {
        var message = await viewUseCase.FocusSceneViewAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Switch focus to the Game View window.</summary>
    [Command("focus-game")]
    public async Task FocusGame(CancellationToken cancellationToken = default)
    {
        var message = await viewUseCase.FocusGameViewAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
