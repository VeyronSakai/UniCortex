using ModelContextProtocol.Protocol;
using UniCortex.Core.Domains.Interfaces;

namespace UniCortex.Mcp.Tools;

internal static class McpToolExecution
{
    // Serialize whole MCP tool calls at the MCP boundary rather than relying only on
    // Unity-side HTTP request ordering. A single tool call can span multiple HTTP
    // requests (wait-for-server, polling, follow-up fetches), so request-level FIFO
    // inside Unity does not guarantee FIFO completion order between tool calls.
    internal static ValueTask<CallToolResult> ExecuteAsync(
        IAsyncOperationSequencer sequencer,
        Func<CancellationToken, ValueTask<CallToolResult>> operation,
        CancellationToken cancellationToken = default)
    {
        return sequencer.EnqueueAsync(async ct =>
        {
            try
            {
                return await operation(ct);
            }
            catch (Exception ex)
            {
                return ToolErrorHandling.CreateErrorResult(ex);
            }
        }, cancellationToken);
    }

    internal static ValueTask<CallToolResult> ExecuteTextAsync(
        IAsyncOperationSequencer sequencer,
        Func<CancellationToken, ValueTask<string>> operation,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(sequencer, async ct => CreateTextResult(await operation(ct)), cancellationToken);
    }

    // For tools that run locally (no Unity Editor HTTP requests). They do not contend
    // with Editor operations, so they bypass the sequencer instead of queueing behind
    // potentially long-running Unity calls.
    internal static async ValueTask<CallToolResult> ExecuteLocalTextAsync(
        Func<CancellationToken, ValueTask<string>> operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return CreateTextResult(await operation(cancellationToken));
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    internal static CallToolResult CreateTextResult(string text)
    {
        return new CallToolResult { Content = [new TextContentBlock { Text = text }] };
    }
}
 