using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ProjectWindowTools(ProjectWindowUseCase projectWindowUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "select_project_window_asset", ReadOnly = false),
     Description("Select an asset in the Unity Project Window, focus the window, and ping the asset."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SelectProjectWindowAssetAsync(
        [Description("Asset path to select, for example Assets/Scenes/Main.unity.")]
        string assetPath,
        CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => projectWindowUseCase.SelectAsync(assetPath, ct), cancellationToken);
}
