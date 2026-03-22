using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class GameViewCommands(GameViewUseCase gameViewUseCase)
{
    /// <summary>Capture a screenshot of the Game View as a PNG file. Only available in Play Mode.</summary>
    /// <param name="outputPath">File path to save the PNG screenshot.</param>
    [Command("capture")]
    public async Task Capture([Argument] string outputPath, CancellationToken cancellationToken = default)
    {
        var pngData = await gameViewUseCase.CaptureAsync(cancellationToken);
        await File.WriteAllBytesAsync(outputPath, pngData, cancellationToken);
        Console.WriteLine($"Screenshot saved to: {outputPath}");
    }
}
