using System.ComponentModel;
using System.Text.Json;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class MovieRecordingTools(MovieRecordingUseCase movieRecordingUseCase)
{
    [McpServerTool(Name = "add_movie_recorder", ReadOnly = false),
     Description(
         "Add a Movie recorder to the Movie recorder list. " +
         "Records the Game View with audio. Output format depends on the chosen encoder (MP4 by default). " +
         "Returns the assigned recorder name. " +
         "Requires the Unity Recorder package (com.unity.recorder) to be installed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> AddRecorderAsync(
        [Description("Name for the recorder (required).")]
        string name,
        [Description("Output file path for the video (required).")]
        string outputPath,
        [Description("Encoder: UnityMediaEncoder (default), ProRes, GIF")]
        string encoder = MovieRecorderEncoderType.UnityMediaEncoder,
        [Description("Encoding quality (UnityMediaEncoder only): Low (default), Medium, High")]
        string encodingQuality = MovieRecorderEncodingQuality.Low,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resultName = await movieRecordingUseCase.AddAsync(
                name, outputPath, encoder, encodingQuality, cancellationToken);
            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"Recorder added: {resultName}" }]
            };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "get_all_recorders", ReadOnly = true),
     Description(
         "Get the list of all configured recorders and their settings (Movie, etc.). " +
         "Requires the Unity Recorder package (com.unity.recorder) to be installed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> GetRecorderListAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await movieRecordingUseCase.GetListAsync(cancellationToken);
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            });
            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = json }]
            };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "remove_movie_recorder", ReadOnly = false),
     Description(
         "Remove a Movie recorder from the Movie recorder list by its index. " +
         "Use get_all_recorders to find the index. " +
         "Requires the Unity Recorder package (com.unity.recorder) to be installed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> RemoveRecorderAsync(
        [Description("The index of the recorder to remove (obtained from get_all_recorders)")]
        int index,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await movieRecordingUseCase.RemoveAsync(index, cancellationToken);
            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"Recorder at index {index} removed." }]
            };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "start_movie_recorder", ReadOnly = false),
     Description(
         "Start recording with the specified Movie recorder. Only available in Play Mode. " +
         "Requires the Unity Recorder package (com.unity.recorder) to be installed. " +
         "Add a Movie recorder first with add_movie_recorder. " +
         "Call stop_movie_recorder to stop and save the recording."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> StartRecorderAsync(
        [Description("The index of the recorder to use (obtained from get_all_recorders)")]
        int index,
        [Description("Frames per second (default: 30)")]
        int fps = RecorderFps.Default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await movieRecordingUseCase.StartAsync(index, fps, cancellationToken);
            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"Recording started with recorder at index {index}." }]
            };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "stop_movie_recorder", ReadOnly = false),
     Description(
         "Stop the current Movie recording and save the video file. " +
         "Returns the output file path where the recording was saved."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> StopRecorderAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var outputPath = await movieRecordingUseCase.StopAsync(cancellationToken);
            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"Recording saved to: {outputPath}" }]
            };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
