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
        [Description("The instance ID of the GameObject to save as a Prefab.")]
        int instanceId,
        [Description("The asset path where the Prefab should be saved (e.g. \"Assets/Prefabs/MyCube.prefab\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => prefabUseCase.CreateAsync(instanceId, assetPath, ct), cancellationToken);

    [McpServerTool(Name = "instantiate_prefab", ReadOnly = false),
     Description("Instantiate a Prefab into the current scene. Returns the new GameObject's name and instance ID."),
     UsedImplicitly]
    public ValueTask<CallToolResult> InstantiatePrefabAsync(
        [Description("The asset path of the Prefab to instantiate (e.g. \"Assets/Prefabs/MyCube.prefab\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => prefabUseCase.InstantiateAsync(assetPath, ct), cancellationToken);

    [McpServerTool(Name = "open_prefab", ReadOnly = false),
     Description("Open a Prefab asset in Prefab Mode for editing."), UsedImplicitly]
    public ValueTask<CallToolResult> OpenPrefabAsync(
        [Description("The asset path of the Prefab to open (e.g. \"Assets/Prefabs/MyCube.prefab\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => prefabUseCase.OpenAsync(assetPath, ct), cancellationToken);

    [McpServerTool(Name = "close_prefab", ReadOnly = false),
     Description("Close Prefab Mode and return to the main stage."), UsedImplicitly]
    public ValueTask<CallToolResult> ClosePrefabAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer, prefabUseCase.CloseAsync, cancellationToken);
}
