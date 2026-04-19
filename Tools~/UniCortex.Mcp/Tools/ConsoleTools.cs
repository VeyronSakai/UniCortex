using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ConsoleTools(ConsoleUseCase consoleUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "get_console_logs", ReadOnly = true),
     Description("Get console log entries from the Unity Editor."), UsedImplicitly]
    public ValueTask<CallToolResult> GetConsoleLogsAsync(
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
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => consoleUseCase.GetLogsAsync(count, stackTrace, log, warning, error, ct),
            cancellationToken);

    [McpServerTool(Name = "clear_console_logs", ReadOnly = false),
     Description("Clear all console logs in the Unity Editor."), UsedImplicitly]
    public ValueTask<CallToolResult> ClearConsoleLogsAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer, consoleUseCase.ClearAsync, cancellationToken);
}
