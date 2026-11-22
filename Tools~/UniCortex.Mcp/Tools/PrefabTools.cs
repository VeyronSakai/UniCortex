using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class PrefabTools(PrefabUseCase prefabUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "create_prefab", ReadOnly = false),
     Description("Create a Prefab asset from a GameObject in the scene."), UsedImplicitly]
    public ValueTask<CallToolResult> CreatePrefabAsync(
        [Description("instanceId of the GameObject to save.")]
        int instanceId,
        [Description("Asset path to save to (e.g. \"Assets/Prefabs/MyCube.prefab\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => prefabUseCase.CreateAsync(instanceId, assetPath, ct), cancellationToken);

    [McpServerTool(Name = "instantiate_prefab", ReadOnly = false),
     Description("Instantiate a Prefab into the current scene. Returns the new GameObject's name and instance ID."),
     UsedImplicitly]
    public ValueTask<CallToolResult> InstantiatePrefabAsync(
        [Description("Prefab asset path (e.g. \"Assets/Prefabs/MyCube.prefab\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => prefabUseCase.InstantiateAsync(assetPath, ct), cancellationToken);

    [McpServerTool(Name = "open_prefab", ReadOnly = false),
     Description("Open a Prefab asset in Prefab Mode for editing. " +
                 "Fails while the Prefab already open in Prefab Mode has unsaved changes; " +
                 "save or discard them first."),
     UsedImplicitly]
    public ValueTask<CallToolResult> OpenPrefabAsync(
        [Description("Prefab asset path (e.g. \"Assets/Prefabs/MyCube.prefab\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => prefabUseCase.OpenAsync(assetPath, ct), cancellationToken);

    [McpServerTool(Name = "close_prefab", ReadOnly = false),
     Description("Close Prefab Mode and return to the main stage. " +
                 "Fails while the Prefab has unsaved changes, unless discardChanges is true; " +
                 "save them first to keep them."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ClosePrefabAsync(
        [Description("Discard the unsaved changes of the Prefab instead of failing. Cannot be undone.")]
        bool discardChanges = false,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => prefabUseCase.CloseAsync(discardChanges, ct), cancellationToken);
}
