using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class SceneViewTools(SceneViewUseCase sceneViewUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "focus_scene_view", ReadOnly = false),
     Description("Switch focus to the Scene View window in the Unity Editor."),
     UsedImplicitly]
    public ValueTask<CallToolResult> FocusSceneViewAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, sceneViewUseCase.FocusAsync, cancellationToken);

    [McpServerTool(Name = "capture_scene_view", ReadOnly = true),
     Description(
         "Capture the Scene View as a PNG image, rendered from the Scene View camera. " +
         "Available in both Edit Mode and Play Mode, and captures the Prefab contents in Prefab Mode. " +
         "Gizmos, grid and Screen Space - Overlay UI are not included."),
     UsedImplicitly]
    public ValueTask<CallToolResult> CaptureSceneViewAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            var pngData = await sceneViewUseCase.CaptureAsync(ct);
            return new CallToolResult
            {
                Content = [ImageContentBlock.FromBytes(pngData, "image/png")]
            };
        }, cancellationToken);
}
