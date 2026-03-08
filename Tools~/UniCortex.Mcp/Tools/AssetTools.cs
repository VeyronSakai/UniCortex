using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Services;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class AssetTools(AssetService assetService)
{
    [McpServerTool(Name = "refresh_asset_database", ReadOnly = false),
     Description("Refresh the Unity Asset Database."), UsedImplicitly]
    public async ValueTask<CallToolResult> RefreshAssetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await assetService.RefreshAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
