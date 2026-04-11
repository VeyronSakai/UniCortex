using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

internal static class CustomToolDynamicHandler
{
    internal static async ValueTask<ListToolsResult> ListToolsAsync(
        RequestContext<ListToolsRequestParams> context, CancellationToken cancellationToken)
    {
        var useCase = context.Server.Services!.GetRequiredService<CustomToolUseCase>();
        var logger = context.Server.Services!.GetRequiredService<ILogger<CustomToolUseCase>>();

        try
        {
            var response = await useCase.ListAsync(cancellationToken);
            var tools = new List<Tool>();

            foreach (var info in response.tools)
            {
                var tool = new Tool
                {
                    Name = info.name,
                    Description = info.description,
                    Annotations = new ToolAnnotations { ReadOnlyHint = info.readOnly }
                };

                if (!string.IsNullOrEmpty(info.inputSchema))
                {
                    tool.InputSchema = JsonSerializer.Deserialize<JsonElement>(info.inputSchema);
                }

                tools.Add(tool);
            }

            return new ListToolsResult { Tools = tools };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to list custom tools from Unity Editor");
            return new ListToolsResult { Tools = [] };
        }
    }

    internal static async ValueTask<CallToolResult> CallToolAsync(
        RequestContext<CallToolRequestParams> context, CancellationToken cancellationToken)
    {
        var useCase = context.Server.Services!.GetRequiredService<CustomToolUseCase>();

        try
        {
            var name = context.Params!.Name;
            string? argumentsJson = null;

            if (context.Params.Arguments is { Count: > 0 })
            {
                argumentsJson = JsonSerializer.Serialize(context.Params.Arguments);
            }

            var result = await useCase.ExecuteAsync(name, argumentsJson, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = result }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
