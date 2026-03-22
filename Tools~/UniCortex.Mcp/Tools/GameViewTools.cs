using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class GameViewTools(GameViewUseCase gameViewService)
{
    [McpServerTool(Name = "capture_game_view", ReadOnly = true),
     Description("Capture a screenshot of the Game View as a PNG image. " +
                 "Only available in Play Mode. Renders from the main camera."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> CaptureGameViewAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pngData = await gameViewService.CaptureAsync(cancellationToken);
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
