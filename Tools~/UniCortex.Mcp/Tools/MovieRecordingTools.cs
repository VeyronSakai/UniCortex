using System.ComponentModel;
using System.Text.Json;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class MovieRecordingTools(MovieRecordingUseCase movieRecordingUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "add_movie_recorder", ReadOnly = false),
     Description(
         "Add a Movie recorder that records the Game View and return its name. " +
         "The output format depends on the encoder (MP4 by default). Requires com.unity.recorder."),
     UsedImplicitly]
    public ValueTask<CallToolResult> AddRecorderAsync(
        [Description("Recorder name.")]
        string name,
        [Description("Output file path of the video.")]
        string outputPath,
        [Description("Encoder: UnityMediaEncoder, ProRes or GIF.")]
        string encoder = MovieRecorderEncoderType.UnityMediaEncoder,
        [Description("Encoding quality (UnityMediaEncoder only): Low, Medium or High.")]
        string encodingQuality = MovieRecorderEncodingQuality.Low,
        [Description("Capture audio.")]
        bool captureAudio = false,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            var resultName = await movieRecordingUseCase.AddAsync(
                name, outputPath, encoder, encodingQuality, captureAudio, ct);
            return McpToolExecution.CreateTextResult($"Recorder added: {resultName}");
        }, cancellationToken);

    [McpServerTool(Name = "get_all_recorders", ReadOnly = true),
     Description(
         "List the configured recorders and their settings. Requires com.unity.recorder."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetRecorderListAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            var response = await movieRecordingUseCase.GetListAsync(ct);
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            });
            return McpToolExecution.CreateTextResult(json);
        }, cancellationToken);

    [McpServerTool(Name = "remove_movie_recorder", ReadOnly = false),
     Description(
         "Remove a Movie recorder by its index from get_all_recorders. Requires com.unity.recorder."),
     UsedImplicitly]
    public ValueTask<CallToolResult> RemoveRecorderAsync(
        [Description("Recorder index from get_all_recorders.")]
        int index,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            await movieRecordingUseCase.RemoveAsync(index, ct);
            return McpToolExecution.CreateTextResult($"Recorder at index {index} removed.");
        }, cancellationToken);

    [McpServerTool(Name = "start_movie_recorder", ReadOnly = false),
     Description(
         "Start recording with a Movie recorder (added with add_movie_recorder) in Play Mode; " +
         "stop and save with stop_movie_recorder. Requires com.unity.recorder."),
     UsedImplicitly]
    public ValueTask<CallToolResult> StartRecorderAsync(
        [Description("Recorder index from get_all_recorders.")]
        int index,
        [Description("Frames per second.")]
        int fps = RecorderFps.Default,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            await movieRecordingUseCase.StartAsync(index, fps, ct);
            return McpToolExecution.CreateTextResult($"Recording started with recorder at index {index}.");
        }, cancellationToken);

    [McpServerTool(Name = "stop_movie_recorder", ReadOnly = false),
     Description(
         "Stop the current Movie recording, save the video and return its file path."),
     UsedImplicitly]
    public ValueTask<CallToolResult> StopRecorderAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            var outputPath = await movieRecordingUseCase.StopAsync(ct);
            return McpToolExecution.CreateTextResult($"Recording saved to: {outputPath}");
        }, cancellationToken);
}
