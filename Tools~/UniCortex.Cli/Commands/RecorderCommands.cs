using System.Text.Json;
using ConsoleAppFramework;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Commands;

public class RecorderCommands(RecordingUseCase recordingUseCase)
{
    /// <summary>Add a Movie recorder to the recorder list. Records Game View with audio. Requires com.unity.recorder.</summary>
    /// <param name="name">Name for the recorder (required).</param>
    /// <param name="outputPath">Output file path for the video (required).</param>
    /// <param name="encoder">Encoder: UnityMediaEncoder (default), ProRes, GIF.</param>
    /// <param name="quality">Encoding quality (UnityMediaEncoder only): Low (default), Medium, High.</param>
    [Command("add")]
    public async Task Add(
        string name, string outputPath,
        string encoder = RecorderEncoderType.UnityMediaEncoder,
        string quality = RecorderEncodingQuality.Low,
        CancellationToken cancellationToken = default)
    {
        var resultName = await recordingUseCase.AddAsync(
            name, outputPath, encoder, quality, cancellationToken);
        Console.WriteLine($"Recorder added: {resultName}");
    }

    /// <summary>Get the list of configured recorders. Requires com.unity.recorder.</summary>
    [Command("list")]
    public async Task List(CancellationToken cancellationToken = default)
    {
        var response = await recordingUseCase.GetListAsync(cancellationToken);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        });
        Console.WriteLine(json);
    }

    /// <summary>Remove a recorder from the recorder list by index. Requires com.unity.recorder.</summary>
    /// <param name="index">The index of the recorder to remove (use 'list' to find it).</param>
    [Command("remove")]
    public async Task Remove(int index, CancellationToken cancellationToken = default)
    {
        await recordingUseCase.RemoveAsync(index, cancellationToken);
        Console.WriteLine($"Recorder at index {index} removed.");
    }

    /// <summary>Start recording with the specified recorder. Only available in Play Mode. Requires com.unity.recorder.</summary>
    /// <param name="index">The index of the recorder to use (use 'list' to find it).</param>
    /// <param name="fps">Frames per second (default: 30).</param>
    [Command("start")]
    public async Task Start(
        int index, int fps = RecorderFps.Default,
        CancellationToken cancellationToken = default)
    {
        await recordingUseCase.StartAsync(index, fps, cancellationToken);
        Console.WriteLine($"Recording started with recorder at index {index}.");
    }

    /// <summary>Stop the current recording and save the video file.</summary>
    [Command("stop")]
    public async Task Stop(CancellationToken cancellationToken = default)
    {
        var outputPath = await recordingUseCase.StopAsync(cancellationToken);
        Console.WriteLine($"Recording saved to: {outputPath}");
    }
}
