using ModelContextProtocol.Protocol;

namespace UniCortex.Mcp.Tools;

internal static class ToolErrorHandling
{
    internal static CallToolResult CreateErrorResult(Exception ex)
    {
        var message = string.IsNullOrWhiteSpace(ex.Message) ? "Unexpected error." : ex.Message;
        return new CallToolResult
        {
            IsError = true,
            Content = [new TextContentBlock { Text = message }]
        };
    }
}
