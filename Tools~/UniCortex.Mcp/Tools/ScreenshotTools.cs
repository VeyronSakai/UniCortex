using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ScreenshotTools(ScreenshotUseCase screenshotUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "capture_screenshot", ReadOnly = true),
     Description(
         "Capture a screenshot of the current Unity rendering output as a PNG image. Only available in Play Mode. " +
         "Typically captures the Game View, but may capture the Scene View if the Game View is not focused or not open."),
     UsedImplicitly]
    public ValueTask<CallToolResult> CaptureScreenshotAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            var pngData = await screenshotUseCase.CaptureAsync(ct);
            return new CallToolResult
            {
                Content = [ImageContentBlock.FromBytes(pngData, "image/png")]
            };
        }, cancellationToken);
}
