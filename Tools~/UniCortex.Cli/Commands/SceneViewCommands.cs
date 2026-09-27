using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class SceneViewCommands(SceneViewUseCase sceneViewUseCase)
{
    /// <summary>Switch focus to the Scene View window.</summary>
    [Command("focus")]
    public async Task Focus(CancellationToken cancellationToken = default)
    {
        var message = await sceneViewUseCase.FocusAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Capture the Scene View as a PNG file. Available in both Edit Mode and Play Mode, including Prefab Mode.</summary>
    /// <param name="outputPath">File path to save the PNG image.</param>
    [Command("capture")]
    public async Task Capture([Argument] string outputPath, CancellationToken cancellationToken = default)
    {
        var pngData = await sceneViewUseCase.CaptureAsync(cancellationToken);
        await File.WriteAllBytesAsync(outputPath, pngData, cancellationToken);
        Console.WriteLine($"Scene View captured to: {outputPath}");
    }
}
