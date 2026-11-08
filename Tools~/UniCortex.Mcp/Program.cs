using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains;
using UniCortex.Core.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddUniCortexCore();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithListToolsHandler(UniCortex.Mcp.Tools.ExtensionTools.ListToolsAsync)
    .WithCallToolHandler(UniCortex.Mcp.Tools.ExtensionTools.CallToolAsync);

// Shrink the generated input schemas, which AI agents read as tokens.
builder.Services.PostConfigure<McpServerOptions>(options =>
{
    if (options.ToolCollection is null)
    {
        return;
    }

    foreach (var tool in options.ToolCollection)
    {
        tool.ProtocolTool.InputSchema = ToolInputSchema.Simplify(tool.ProtocolTool.InputSchema);
    }
});

await builder.Build().RunAsync();
