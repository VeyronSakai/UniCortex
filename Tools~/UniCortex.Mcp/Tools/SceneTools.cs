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
                 "Fails while a loaded scene or the Prefab in Prefab Mode has unsaved changes; " +
                 "save or discard them first."), UsedImplicitly]
    public ValueTask<CallToolResult> CreateSceneAsync(
        [Description("Asset path to save to (e.g. \"Assets/Scenes/NewScene.unity\").")]
        string scenePath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => sceneUseCase.CreateAsync(scenePath, ct), cancellationToken);

    [McpServerTool(Name = "open_scene", ReadOnly = false),
     Description("Open a scene in the Unity Editor by its asset path. " +
                 "Fails while a loaded scene or the Prefab in Prefab Mode has unsaved changes; " +
                 "save or discard them first."), UsedImplicitly]
    public ValueTask<CallToolResult> OpenSceneAsync(
        [Description("Scene asset path (e.g. \"Assets/Scenes/Main.unity\").")]
        string scenePath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => sceneUseCase.OpenAsync(scenePath, ct), cancellationToken);

    [McpServerTool(Name = "discard_scene_changes", ReadOnly = false),
     Description("Discard the unsaved changes of every loaded scene by reloading it from disk. " +
                 "Fails without changing anything while an untitled scene has unsaved changes. Cannot be undone."),
     UsedImplicitly]
    public ValueTask<CallToolResult> DiscardSceneChangesAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer, sceneUseCase.DiscardChangesAsync, cancellationToken);

    [McpServerTool(Name = "save_scene_as", ReadOnly = false),
     Description("Save the active scene to the specified asset path without the save panel, like File > Save As. " +
                 "Use it to save an untitled scene. The scene then refers to the new path, and the file at its " +
                 "previous path keeps its last saved state. Fails if the path already exists."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SaveSceneAsAsync(
        [Description("Asset path to save to (e.g. \"Assets/Scenes/NewScene.unity\").")]
        string scenePath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => sceneUseCase.SaveAsAsync(scenePath, ct), cancellationToken);

    [McpServerTool(Name = "get_hierarchy", ReadOnly = true),
     Description("Get the GameObject tree of every loaded scene (DontDestroyOnLoad included in Play Mode), " +
                 "or of the Prefab in Prefab Mode, with each scene's name, path and whether it is active."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetHierarchyAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer, sceneUseCase.GetHierarchyAsync, cancellationToken);
}
