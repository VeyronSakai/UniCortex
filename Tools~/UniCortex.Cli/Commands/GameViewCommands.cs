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

    /// <summary>List all available Game View sizes (built-in and custom).</summary>
    [Command("size-list")]
    public async Task SizeList(CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.GetSizeListAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Set the Game View resolution by index from the size list.</summary>
    /// <param name="index">Index of the size from the size list.</param>
    [Command("set-size-index")]
    public async Task SetSizeIndex(int index, CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.SetSizeByIndexAsync(index, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Set the Game View resolution by width and height in pixels.</summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    [Command("set-size")]
    public async Task SetSize(int width, int height, CancellationToken cancellationToken = default)
    {
        var message = await gameViewUseCase.SetSizeAsync(width, height, cancellationToken);
        Console.WriteLine(message);
    }
}
