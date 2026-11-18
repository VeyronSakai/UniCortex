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
     Description("Create a new empty scene and save it at the specified asset path. " +
                 "Fails while a loaded scene has unsaved changes; save or discard them first."), UsedImplicitly]
    public ValueTask<CallToolResult> CreateSceneAsync(
        [Description("Asset path to save to (e.g. \"Assets/Scenes/NewScene.unity\").")]
        string scenePath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => sceneUseCase.CreateAsync(scenePath, ct), cancellationToken);

    [McpServerTool(Name = "open_scene", ReadOnly = false),
     Description("Open a scene in the Unity Editor by its asset path. " +
                 "Fails while a loaded scene has unsaved changes; save or discard them first."), UsedImplicitly]
    public ValueTask<CallToolResult> OpenSceneAsync(
        [Description("Scene asset path (e.g. \"Assets/Scenes/Main.unity\").")]
        string scenePath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => sceneUseCase.OpenAsync(scenePath, ct), cancellationToken);

    [McpServerTool(Name = "discard_scene_changes", ReadOnly = false),
     Description("Discard the unsaved changes of every loaded scene: scenes with a path are reopened from disk, " +
                 "and untitled scenes are closed (replaced with a new empty scene if no other scene remains). " +
                 "Cannot be undone."), UsedImplicitly]
    public ValueTask<CallToolResult> DiscardSceneChangesAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer, sceneUseCase.DiscardChangesAsync, cancellationToken);

    [McpServerTool(Name = "get_hierarchy", ReadOnly = true),
     Description("Get the GameObject tree of every loaded scene (DontDestroyOnLoad included in Play Mode), " +
                 "or of the Prefab in Prefab Mode, with each scene's name, path and whether it is active."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetHierarchyAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer, sceneUseCase.GetHierarchyAsync, cancellationToken);
}
