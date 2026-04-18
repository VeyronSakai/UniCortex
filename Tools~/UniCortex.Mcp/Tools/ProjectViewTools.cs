using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ProjectViewTools(ProjectViewUseCase projectViewUseCase)
{
    [McpServerTool(Name = "select_project_view_asset", ReadOnly = false),
     Description("Select an asset in the Unity Project view, focus the Project window, and ping the asset."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SelectProjectViewAssetAsync(
        [Description("Asset path to select, for example Assets/Scenes/Main.unity.")]
        string assetPath,
        CancellationToken cancellationToken)
    {
        try
        {
            var message = await projectViewUseCase.SelectAsync(assetPath, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
