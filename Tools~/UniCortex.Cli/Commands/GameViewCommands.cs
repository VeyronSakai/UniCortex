using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class GameViewCommands(ScreenshotUseCase screenshotService)
{
    /// <summary>Capture a screenshot of the Game View as a PNG file.</summary>
    /// <param name="outputPath">File path to save the PNG screenshot.</param>
    [Command("capture")]
    public async Task Capture(string outputPath, CancellationToken cancellationToken = default)
    {
        var pngData = await screenshotService.CaptureGameViewAsync(cancellationToken);
        await File.WriteAllBytesAsync(outputPath, pngData, cancellationToken);
        Console.WriteLine($"Screenshot saved to: {outputPath}");
    }
}
