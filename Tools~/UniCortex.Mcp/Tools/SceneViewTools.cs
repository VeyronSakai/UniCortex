using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class SceneViewTools(SceneViewUseCase sceneViewUseCase)
{
    [McpServerTool(Name = "focus_scene_view", ReadOnly = false),
     Description("Switch focus to the Scene View window in the Unity Editor."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> FocusSceneViewAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await sceneViewUseCase.FocusAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
