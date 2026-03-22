using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class SceneViewTools(SceneViewUseCase sceneViewService)
{
    [McpServerTool(Name = "capture_scene_view", ReadOnly = true),
     Description("Capture a screenshot of the Scene View as a PNG image. Also works in Prefab Mode."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> CaptureSceneViewAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pngData = await sceneViewService.CaptureAsync(cancellationToken);
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
