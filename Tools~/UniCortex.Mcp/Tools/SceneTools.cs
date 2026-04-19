using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class SceneTools(SceneUseCase sceneUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "create_scene", ReadOnly = false),
     Description("Create a new empty scene and save it at the specified asset path."), UsedImplicitly]
    public ValueTask<CallToolResult> CreateSceneAsync(
        [Description("The asset path where the scene should be saved (e.g. \"Assets/Scenes/NewScene.unity\").")]
        string scenePath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => sceneUseCase.CreateAsync(scenePath, ct), cancellationToken);

    [McpServerTool(Name = "open_scene", ReadOnly = false),
     Description("Open a scene in the Unity Editor by its asset path."), UsedImplicitly]
    public ValueTask<CallToolResult> OpenSceneAsync(
        [Description("The asset path of the scene to open (e.g. \"Assets/Scenes/Main.unity\").")]
        string scenePath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => sceneUseCase.OpenAsync(scenePath, ct), cancellationToken);

    [McpServerTool(Name = "get_hierarchy", ReadOnly = true),
     Description("Get the GameObject hierarchy of the currently open scene or Prefab in the Unity Editor."), UsedImplicitly]
    public ValueTask<CallToolResult> GetHierarchyAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer, sceneUseCase.GetHierarchyAsync, cancellationToken);
}
