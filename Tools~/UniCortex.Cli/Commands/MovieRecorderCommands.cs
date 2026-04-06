using ConsoleAppFramework;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

#pragma warning disable CS1573 // Parameter has no matching param tag

namespace UniCortex.Cli.Commands;

public class MovieRecorderCommands(MovieRecordingUseCase movieRecordingUseCase)
{
    /// <summary>Add a Movie recorder to the Movie recorder list. Records Game View with audio. Requires com.unity.recorder.</summary>
    /// <param name="name">Name for the recorder (required).</param>
    /// <param name="outputPath">Output file path for the video (required).</param>
    /// <param name="encoder">Encoder: UnityMediaEncoder (default), ProRes, GIF.</param>
    /// <param name="encodingQuality">Encoding quality (UnityMediaEncoder only): Low (default), Medium, High.</param>
    [Command("add")]
    public async Task Add(
        [Argument] string name, [Argument] string outputPath,
        string encoder = MovieRecorderEncoderType.UnityMediaEncoder,
        string encodingQuality = MovieRecorderEncodingQuality.Low,
        CancellationToken cancellationToken = default)
    {
        var resultName = await movieRecordingUseCase.AddAsync(
            name, outputPath, encoder, encodingQuality, cancellationToken);
        Console.WriteLine($"Recorder added: {resultName}");
    }

    /// <summary>Remove a Movie recorder by index. Use 'recorder all list' to find the index. Requires com.unity.recorder.</summary>
    /// <param name="index">The index of the recorder to remove (use 'recorder all list' to find it).</param>
    [Command("remove")]
    public async Task Remove([Argument] int index, CancellationToken cancellationToken = default)
    {
        await movieRecordingUseCase.RemoveAsync(index, cancellationToken);
        Console.WriteLine($"Recorder at index {index} removed.");
    }

    /// <summary>Start recording with the specified Movie recorder. Only available in Play Mode. Requires com.unity.recorder.</summary>
    /// <param name="index">The index of the recorder to use (use 'recorder all list' to find it).</param>
    /// <param name="fps">Frames per second (default: 30).</param>
    [Command("start")]
    public async Task Start(
        [Argument] int index, int fps = RecorderFps.Default,
        CancellationToken cancellationToken = default)
    {
        await movieRecordingUseCase.StartAsync(index, fps, cancellationToken);
        Console.WriteLine($"Recording started with recorder at index {index}.");
    }

    /// <summary>Stop the current Movie recording and save the video file.</summary>
    [Command("stop")]
    public async Task Stop(CancellationToken cancellationToken = default)
    {
        var outputPath = await movieRecordingUseCase.StopAsync(cancellationToken);
        Console.WriteLine($"Recording saved to: {outputPath}");
    }
}
