using ConsoleAppFramework;
using UniCortex.Core.Services;

namespace UniCortex.Cli.Commands;

public class ScreenshotCommands(ScreenshotService screenshotService)
{
    /// <summary>Capture a screenshot of the Game View as a PNG file. Only available in Play Mode.</summary>
    [Command("capture")]
    public async Task Capture(string outputPath, CancellationToken cancellationToken = default)
    {
        var pngData = await screenshotService.CaptureAsync(cancellationToken);
        await File.WriteAllBytesAsync(outputPath, pngData, cancellationToken);
        Console.WriteLine($"Screenshot saved to: {outputPath}");
    }
}
