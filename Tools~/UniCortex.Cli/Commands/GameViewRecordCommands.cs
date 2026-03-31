using System.Text.Json;
using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class GameViewRecordCommands(RecordingUseCase recordingUseCase)
{
    /// <summary>Configure the Game View recorder settings. Requires com.unity.recorder.</summary>
    /// <param name="outputPath">Output file path for the video.</param>
    /// <param name="source">Video source: GameView, Camera.</param>
    /// <param name="cameraSource">Camera source: ActiveCamera, MainCamera, TaggedCamera.</param>
    /// <param name="cameraTag">Camera tag for TaggedCamera source.</param>
    /// <param name="captureUI">Capture UI overlay (Camera source only).</param>
    /// <param name="width">Output width in pixels (0 = default).</param>
    /// <param name="height">Output height in pixels (0 = default).</param>
    /// <param name="format">Output format: MP4, WebM.</param>
    [Command("configure")]
    public async Task Configure(
        string? outputPath = null, string? source = null,
        string? cameraSource = null, string? cameraTag = null,
        bool captureUI = false,
        int width = 0, int height = 0, string? format = null,
        CancellationToken cancellationToken = default)
    {
        var message = await recordingUseCase.ConfigureAsync(
            outputPath, source, cameraSource, cameraTag,
            captureUI, width, height, format, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Get the current Game View recorder settings. Requires com.unity.recorder.</summary>
    [Command("settings")]
    public async Task Settings(CancellationToken cancellationToken = default)
    {
        var settings = await recordingUseCase.GetSettingsAsync(cancellationToken);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        });
        Console.WriteLine(json);
    }

    /// <summary>Start recording the Game View. Only available in Play Mode. Requires com.unity.recorder.</summary>
    /// <param name="fps">Frames per second (default: 30).</param>
    /// <param name="playback">Playback mode: Constant, Variable.</param>
    /// <param name="mode">Record mode: Manual, SingleFrame, FrameInterval, TimeInterval.</param>
    /// <param name="startTime">Start time for TimeInterval mode.</param>
    /// <param name="endTime">End time for TimeInterval mode.</param>
    /// <param name="startFrame">Start frame for FrameInterval mode.</param>
    /// <param name="endFrame">End frame for FrameInterval mode.</param>
    /// <param name="frameNumber">Frame number for SingleFrame mode.</param>
    [Command("start")]
    public async Task Start(
        int fps = 30, string? playback = null, string? mode = null,
        float startTime = 0, float endTime = 0,
        int startFrame = 0, int endFrame = 0, int frameNumber = 0,
        CancellationToken cancellationToken = default)
    {
        var message = await recordingUseCase.StartAsync(
            fps, playback, mode,
            startTime, endTime, startFrame, endFrame, frameNumber,
            cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Stop the current Game View recording and save the video file.</summary>
    [Command("stop")]
    public async Task Stop(CancellationToken cancellationToken = default)
    {
        var outputPath = await recordingUseCase.StopAsync(cancellationToken);
        Console.WriteLine($"Recording saved to: {outputPath}");
    }
}
