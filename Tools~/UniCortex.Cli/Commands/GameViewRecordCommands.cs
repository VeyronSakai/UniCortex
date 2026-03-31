using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class GameViewRecordCommands(RecordingUseCase recordingUseCase)
{
    /// <summary>Start recording the Game View as an MP4 video. Only available in Play Mode. Requires com.unity.recorder.</summary>
    /// <param name="fps">Frames per second for the recording (default: 30).</param>
    /// <param name="outputPath">Output file path for the MP4 video. If not specified, saves to the system temp directory.</param>
    [Command("start")]
    public async Task Start(int fps = 30, string? outputPath = null, CancellationToken cancellationToken = default)
    {
        var message = await recordingUseCase.StartAsync(fps, outputPath, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Stop the current Game View recording and save the MP4 video file.</summary>
    [Command("stop")]
    public async Task Stop(CancellationToken cancellationToken = default)
    {
        var outputPath = await recordingUseCase.StopAsync(cancellationToken);
        Console.WriteLine($"Recording saved to: {outputPath}");
    }
}
