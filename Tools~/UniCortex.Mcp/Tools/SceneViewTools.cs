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
}
