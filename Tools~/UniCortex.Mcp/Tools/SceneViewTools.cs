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

    [McpServerTool(Name = "set_scene_view_camera", ReadOnly = false),
     Description(
         "Set the Scene View camera pose in the Unity Editor by directly specifying its world position and rotation quaternion. " +
         "Optionally override the Scene View zoom size and orthographic mode."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SetSceneViewCameraAsync(
        [Description("Scene View camera world position X.")]
        float positionX,
        [Description("Scene View camera world position Y.")]
        float positionY,
        [Description("Scene View camera world position Z.")]
        float positionZ,
        [Description("Scene View camera rotation quaternion X.")]
        float rotationX,
        [Description("Scene View camera rotation quaternion Y.")]
        float rotationY,
        [Description("Scene View camera rotation quaternion Z.")]
        float rotationZ,
        [Description("Scene View camera rotation quaternion W.")]
        float rotationW,
        [Description("Optional Scene View size (zoom). Must be greater than 0 when provided.")]
        float? size = null,
        [Description("Optional orthographic toggle. true for orthographic, false for perspective, omitted to keep the current mode.")]
        bool? orthographic = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await sceneViewUseCase.SetCameraAsync(
                positionX, positionY, positionZ,
                rotationX, rotationY, rotationZ, rotationW,
                size, orthographic,
                cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
