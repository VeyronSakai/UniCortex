using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Mcp.Tools;

internal sealed class CustomToolMcpRegistry
{
    private readonly IReadOnlyDictionary<string, Tool> _toolsByName;
    private readonly CustomToolUseCase _customToolUseCase;

    private CustomToolMcpRegistry(
        IReadOnlyDictionary<string, Tool> toolsByName,
        CustomToolUseCase customToolUseCase)
    {
        _toolsByName = toolsByName;
        _customToolUseCase = customToolUseCase;
    }

    internal static async ValueTask<CustomToolMcpRegistry?> CreateAsync(
        CustomToolUseCase customToolUseCase,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var manifest = await customToolUseCase.GetManifestAsync(cancellationToken);
            var builtInToolNames = GetBuiltInToolNames();
            var toolsByName = new Dictionary<string, Tool>(StringComparer.Ordinal);

            foreach (var tool in manifest.tools.Where(static tool => tool.exposeToMcp))
            {
                if (builtInToolNames.Contains(tool.name))
                {
                    logger.LogWarning(
                        "Skipping custom MCP tool '{ToolName}' because it conflicts with a built-in tool name.",
                        tool.name);
                    continue;
                }

                toolsByName[tool.name] = new Tool
                {
                    Name = tool.name,
                    Description = tool.description,
                    InputSchema = CreateInputSchema(tool)
                };
            }

            return toolsByName.Count == 0
                ? null
                : new CustomToolMcpRegistry(toolsByName, customToolUseCase);
        }
        catch (Exception ex)
        {
            logger.LogWarning("Custom MCP tools were not loaded: {Message}", ex.Message);
            return null;
        }
    }

    internal ValueTask<ListToolsResult> HandleListToolsAsync(
        RequestContext<ListToolsRequestParams> request,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new ListToolsResult { Tools = _toolsByName.Values.ToList() });
    }

    internal async ValueTask<CallToolResult> HandleCallToolAsync(
        RequestContext<CallToolRequestParams> request,
        CancellationToken cancellationToken)
    {
        var toolName = request.Params?.Name;
        if (string.IsNullOrWhiteSpace(toolName) || !_toolsByName.ContainsKey(toolName))
        {
            throw new McpProtocolException($"Unknown tool: '{toolName}'.", McpErrorCode.InvalidRequest);
        }

        try
        {
            var argumentsJson = request.Params?.Arguments is { Count: > 0 } arguments
                ? JsonSerializer.Serialize(arguments, JsonOptions.Default)
                : "{}";

            var content = await _customToolUseCase.InvokeAsync(toolName, argumentsJson, cancellationToken);
            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = content }]
            };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    private static HashSet<string> GetBuiltInToolNames()
    {
        return typeof(CustomToolMcpRegistry).Assembly
            .GetTypes()
            .Where(static type => type.GetCustomAttribute<McpServerToolTypeAttribute>() != null)
            .SelectMany(static type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            .Select(static method => method.GetCustomAttribute<McpServerToolAttribute>())
            .Where(static attribute => attribute != null && !string.IsNullOrWhiteSpace(attribute.Name))
            .Select(static attribute => attribute!.Name!)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static JsonElement CreateInputSchema(CustomToolManifestEntry tool)
    {
        var properties = new JsonObject();
        foreach (var parameter in tool.parameters)
        {
            var property = new JsonObject
            {
                ["type"] = parameter.type
            };

            if (!string.IsNullOrWhiteSpace(parameter.description))
            {
                property["description"] = parameter.description;
            }

            if (parameter.type == "array" && !string.IsNullOrWhiteSpace(parameter.itemType))
            {
                property["items"] = new JsonObject
                {
                    ["type"] = parameter.itemType
                };
            }

            properties[parameter.name] = property;
        }

        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties
        };

        var required = tool.parameters
            .Where(static parameter => parameter.required)
            .Select(static parameter => (JsonNode?)parameter.name)
            .ToArray();

        if (required.Length > 0)
        {
            schema["required"] = new JsonArray(required);
        }

        return JsonSerializer.SerializeToElement(schema);
    }
}
