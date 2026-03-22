using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ScreenshotTools(ScreenshotUseCase screenshotService)
{
    [McpServerTool(Name = "capture_game_view", ReadOnly = true),
     Description("Capture a screenshot of the Game View as a PNG image. " +
                 "In Play Mode, captures the active Game View. In Edit Mode, renders from the scene camera."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> CaptureGameViewAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pngData = await screenshotService.CaptureGameViewAsync(cancellationToken);
            return new CallToolResult
            {
                Content = [ImageContentBlock.FromBytes(pngData, "image/png")]
            };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "capture_scene_view", ReadOnly = true),
     Description("Capture a screenshot of the Scene View as a PNG image. " +
                 "Works in both Edit Mode and Play Mode. Requires a Scene View window to be open."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> CaptureSceneViewAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pngData = await screenshotService.CaptureSceneViewAsync(cancellationToken);
            return new CallToolResult
            {
                Content = [ImageContentBlock.FromBytes(pngData, "image/png")]
            };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
