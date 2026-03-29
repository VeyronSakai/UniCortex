using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class GameViewCommands(ViewUseCase viewUseCase)
{
    /// <summary>Switch focus to the Game View window.</summary>
    [Command("focus")]
    public async Task Focus(CancellationToken cancellationToken = default)
    {
        var message = await viewUseCase.FocusGameViewAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
