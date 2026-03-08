using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Services;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class MenuItemTools(MenuItemService menuItemService)
{
    [McpServerTool(Name = "execute_menu_item", ReadOnly = false),
     Description("Execute a Unity Editor menu item by its path (e.g. \"GameObject/3D Object/Cube\")."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> ExecuteMenuItemAsync(
        [Description("The full menu path (e.g. \"GameObject/3D Object/Cube\", \"File/Save\").")]
        string menuPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await menuItemService.ExecuteAsync(menuPath, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
