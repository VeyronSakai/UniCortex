using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class SceneViewCommands(SceneViewUseCase sceneViewUseCase)
{
    /// <summary>Capture a screenshot of the Scene View as a PNG file. Also works in Prefab Mode.</summary>
    /// <param name="outputPath">File path to save the PNG screenshot.</param>
    [Command("capture")]
    public async Task Capture([Argument] string outputPath, CancellationToken cancellationToken = default)
    {
        var pngData = await sceneViewUseCase.CaptureAsync(cancellationToken);
        await File.WriteAllBytesAsync(outputPath, pngData, cancellationToken);
        Console.WriteLine($"Screenshot saved to: {outputPath}");
    }
}
