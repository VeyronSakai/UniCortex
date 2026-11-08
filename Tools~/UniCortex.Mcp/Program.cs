using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

await builder.Build().RunAsync();
