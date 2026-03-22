using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class SceneViewCommands(SceneViewUseCase sceneViewService)
{
    /// <summary>Capture a screenshot of the Scene View as a PNG file.</summary>
    /// <param name="outputPath">File path to save the PNG screenshot.</param>
    [Command("capture")]
    public async Task Capture(string outputPath, CancellationToken cancellationToken = default)
    {
        var pngData = await sceneViewService.CaptureAsync(cancellationToken);
        await File.WriteAllBytesAsync(outputPath, pngData, cancellationToken);
        Console.WriteLine($"Screenshot saved to: {outputPath}");
    }
}
