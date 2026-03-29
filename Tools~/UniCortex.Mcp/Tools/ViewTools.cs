using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ViewTools(ViewUseCase viewUseCase)
{
    [McpServerTool(Name = "focus_scene_view", ReadOnly = false),
     Description("Switch focus to the Scene View window in the Unity Editor."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> FocusSceneViewAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await viewUseCase.FocusSceneViewAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "focus_game_view", ReadOnly = false),
     Description(
         "Switch focus to the Game View window in the Unity Editor. " +
         "Useful before capture_screenshot to ensure the Game View is captured."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> FocusGameViewAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await viewUseCase.FocusGameViewAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
