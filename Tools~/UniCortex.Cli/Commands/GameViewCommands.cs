using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class GameViewCommands(GameViewUseCase gameViewUseCase)
{
    /// <summary>Switch focus to the Game View window.</summary>
    [Command("focus")]
    public async Task Focus(CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.FocusAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Get the current Game View size (width and height in pixels).</summary>
    [Command("size")]
    public async Task Size(CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.GetSizeAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
