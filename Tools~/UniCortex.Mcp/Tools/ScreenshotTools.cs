using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Services;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ScreenshotTools(ScreenshotService screenshotService)
{
    [McpServerTool(Name = "capture_screenshot", ReadOnly = true),
     Description("Capture a screenshot of the Game View as a PNG image. Only available in Play Mode."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> CaptureScreenshotAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pngData = await screenshotService.CaptureAsync(cancellationToken);
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
