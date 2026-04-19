using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class AssetTools(AssetUseCase assetUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "refresh_asset_database", ReadOnly = false),
     Description("Refresh the Unity Asset Database."), UsedImplicitly]
    public ValueTask<CallToolResult> RefreshAssetDatabaseAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer, assetUseCase.RefreshAsync, cancellationToken);
}
