using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class MenuItemTools(MenuItemUseCase menuItemUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "execute_menu_item", ReadOnly = false),
     Description("Execute a Unity Editor menu item by its path (e.g. \"GameObject/3D Object/Cube\")."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ExecuteMenuItemAsync(
        [Description("The full menu path (e.g. \"GameObject/3D Object/Cube\", \"File/Save\").")]
        string menuPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => menuItemUseCase.ExecuteAsync(menuPath, ct), cancellationToken);
}
