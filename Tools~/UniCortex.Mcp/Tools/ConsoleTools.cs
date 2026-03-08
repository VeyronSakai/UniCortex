using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Services;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ConsoleTools(ConsoleService consoleService)
{
    [McpServerTool(Name = "get_console_logs", ReadOnly = true),
     Description("Get console log entries from the Unity Editor."), UsedImplicitly]
    public async ValueTask<CallToolResult> GetConsoleLogsAsync(
        [Description("Number of recent log entries to retrieve. Defaults to 100.")]
        int? count = null,
        [Description("Include stack traces in the output. Defaults to false.")]
        bool? stackTrace = null,
        [Description("Include Info-level logs. Defaults to true.")]
        bool? log = null,
        [Description("Include Warning-level logs. Defaults to true.")]
        bool? warning = null,
        [Description("Include Error-level logs. Defaults to true.")]
        bool? error = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await consoleService.GetLogsAsync(count, stackTrace, log, warning, error,
                cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = json }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "clear_console_logs", ReadOnly = false),
     Description("Clear all console logs in the Unity Editor."), UsedImplicitly]
    public async ValueTask<CallToolResult> ClearConsoleLogsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await consoleService.ClearAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
