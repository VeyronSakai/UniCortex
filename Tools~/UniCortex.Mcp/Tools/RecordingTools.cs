using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class RecordingTools(RecordingUseCase recordingUseCase)
{
    [McpServerTool(Name = "start_game_view_recorder", ReadOnly = false),
     Description(
         "Start recording the Game View as an MP4 video. Only available in Play Mode. " +
         "Requires the Unity Recorder package (com.unity.recorder) to be installed. " +
         "Call stop_game_view_recorder to stop and save the recording."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> StartGameViewRecordingAsync(
        [Description("Frames per second for the recording. Default: 30.")]
        int fps = 30,
        [Description("Output file path for the MP4 video. If not specified, saves to the system temp directory.")]
        string? outputPath = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await recordingUseCase.StartAsync(fps, outputPath, cancellationToken);
            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = message }]
            };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "stop_game_view_recorder", ReadOnly = false),
     Description(
         "Stop the current Game View recording and save the MP4 video file. " +
         "Returns the output file path where the recording was saved."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> StopGameViewRecordingAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var outputPath = await recordingUseCase.StopAsync(cancellationToken);
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
