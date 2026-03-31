using System.ComponentModel;
using System.Text.Json;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class RecordingTools(RecordingUseCase recordingUseCase)
{
    [McpServerTool(Name = "configure_game_view_recorder", ReadOnly = false),
     Description(
         "Configure the Game View recorder settings. Settings persist until changed. " +
         "Requires the Unity Recorder package (com.unity.recorder) to be installed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> ConfigureGameViewRecorderAsync(
        [Description("Output file path for the video. If not specified, saves to the system temp directory.")]
        string? outputPath = null,
        [Description("Video source: GameView, Camera")]
        string? source = null,
        [Description("Camera source when source=Camera: ActiveCamera, MainCamera, TaggedCamera")]
        string? cameraSource = null,
        [Description("Camera tag when cameraSource=TaggedCamera")]
        string? cameraTag = null,
        [Description("Capture UI overlay (only for Camera source)")]
        bool captureUI = false,
        [Description("Output format: MP4, WebM")]
        string? outputFormat = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await recordingUseCase.ConfigureAsync(
                outputPath, source, cameraSource, cameraTag,
                captureUI, outputFormat,
                cancellationToken);
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

    [McpServerTool(Name = "get_game_view_recorder_settings", ReadOnly = true),
     Description(
         "Get the current Game View recorder settings. " +
         "Requires the Unity Recorder package (com.unity.recorder) to be installed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> GetGameViewRecorderSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await recordingUseCase.GetSettingsAsync(cancellationToken);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
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

    [McpServerTool(Name = "start_game_view_recorder", ReadOnly = false),
     Description(
         "Start recording the Game View. Only available in Play Mode. " +
         "Requires the Unity Recorder package (com.unity.recorder) to be installed. " +
         "Configure settings first with configure_game_view_recorder. " +
         "Call stop_game_view_recorder to stop and save the recording."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> StartGameViewRecorderAsync(
        [Description("Frames per second (default: 30)")]
        int fps = 30,
        [Description("Playback mode: Constant, Variable (default: Constant)")]
        string? frameRatePlayback = null,
        [Description("Record mode: Manual, SingleFrame, FrameInterval, TimeInterval (default: Manual)")]
        string? recordMode = null,
        [Description("Start time in seconds for TimeInterval mode")]
        float startTime = 0,
        [Description("End time in seconds for TimeInterval mode")]
        float endTime = 0,
        [Description("Start frame for FrameInterval mode")]
        int startFrame = 0,
        [Description("End frame for FrameInterval mode")]
        int endFrame = 0,
        [Description("Frame number for SingleFrame mode")]
        int frameNumber = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await recordingUseCase.StartAsync(
                fps, frameRatePlayback, recordMode,
                startTime, endTime, startFrame, endFrame, frameNumber,
                cancellationToken);
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
         "Stop the current Game View recording and save the video file. " +
         "Returns the output file path where the recording was saved."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> StopGameViewRecorderAsync(
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
