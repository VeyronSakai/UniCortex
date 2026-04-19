using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

internal static class ExtensionTools
{
    internal static async ValueTask<ListToolsResult> ListToolsAsync(
        RequestContext<ListToolsRequestParams> context, CancellationToken cancellationToken)
    {
        if (context.Server.Services is not { } services)
        {
            return new ListToolsResult { Tools = [] };
        }

        var useCase = services.GetRequiredService<ExtensionUseCase>();
        var logger = services.GetRequiredService<ILogger<ExtensionUseCase>>();

        List<Tool> tools;
        try
        {
            var response = await useCase.ListAsync(cancellationToken);
            var extensions = response.extensions ?? [];
            tools = new List<Tool>(extensions.Count);

            foreach (var info in extensions)
            {
                try
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
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to process extension '{Name}', skipping", info.name);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to list extensions from Unity Editor");
            tools = [];
        }

        return new ListToolsResult { Tools = tools };
    }

    internal static async ValueTask<CallToolResult> CallToolAsync(
        RequestContext<CallToolRequestParams> context, CancellationToken cancellationToken)
    {
        if (context.Server.Services is not { } services)
        {
            return ToolErrorHandling.CreateErrorResult(
                new InvalidOperationException("MCP server services are not available."));
        }

        var sequencer = services.GetRequiredService<IAsyncOperationSequencer>();

        return await McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            if (context.Params is not { } parameters)
            {
                throw new InvalidOperationException("Tool call parameters are missing.");
            }

            var useCase = services.GetRequiredService<ExtensionUseCase>();

            string? argumentsJson = null;
            if (parameters.Arguments is { Count: > 0 })
            {
                argumentsJson = JsonSerializer.Serialize(parameters.Arguments);
            }

            var result = await useCase.ExecuteAsync(parameters.Name, argumentsJson, ct);
            return McpToolExecution.CreateTextResult(result);
        }, cancellationToken);
    }
}
