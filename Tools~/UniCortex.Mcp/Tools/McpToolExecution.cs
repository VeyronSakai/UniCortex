using ModelContextProtocol.Protocol;
using UniCortex.Core.Domains.Interfaces;

namespace UniCortex.Mcp.Tools;

internal static class McpToolExecution
{
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

    internal static CallToolResult CreateTextResult(string text)
    {
        return new CallToolResult { Content = [new TextContentBlock { Text = text }] };
    }
}
 