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
     Description("Create a Prefab asset from a GameObject in a scene or in Prefab Mode. Returns the asset's GUID."),
     UsedImplicitly]
    public ValueTask<CallToolResult> CreatePrefabAsync(
        [Description("instanceId of the GameObject to save.")]
        int instanceId,
        [Description("Asset path to save to (e.g. \"Assets/Prefabs/MyCube.prefab\").")]
        string assetPath,
        [Description("Turn the GameObject into an instance of the new Prefab (references to it stay valid), " +
                     "e.g. to extract part of a Prefab in Prefab Mode. Undoable; the asset is kept.")]
        bool connect = false,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => prefabUseCase.CreateAsync(instanceId, assetPath, connect, ct), cancellationToken);

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

    [McpServerTool(Name = "unpack_prefab_instance", ReadOnly = false),
     Description("Unpack a prefab instance (in a scene or in Prefab Mode) into plain GameObjects, " +
                 "e.g. to edit it freely or save it as a new Prefab with create_prefab. Undoable."),
     UsedImplicitly]
    public ValueTask<CallToolResult> UnpackPrefabInstanceAsync(
        [Description("instanceId of the outermost root of the prefab instance.")]
        int instanceId,
        [Description("\"OutermostRoot\" (default): nested prefab instances inside stay linked. " +
                     "\"Completely\": unpack them too.")]
        string mode = "OutermostRoot",
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => prefabUseCase.UnpackAsync(instanceId, mode, ct), cancellationToken);
}
