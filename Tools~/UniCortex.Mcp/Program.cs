using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UniCortex.Core.Extensions;
using UniCortex.Core.UseCases;
using UniCortex.Mcp.Tools;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddUniCortexCore();

var bootstrapServices = new ServiceCollection();
bootstrapServices.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole(options =>
    {
        options.LogToStandardErrorThreshold = LogLevel.Trace;
    });
});
bootstrapServices.AddUniCortexCore();

var bootstrapProvider = bootstrapServices.BuildServiceProvider();
var customToolRegistry = await CustomToolMcpRegistry.CreateAsync(
    bootstrapProvider.GetRequiredService<CustomToolUseCase>(),
    bootstrapProvider.GetRequiredService<ILoggerFactory>().CreateLogger("UniCortex.Mcp.CustomTools"));

var mcpBuilder = builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

if (customToolRegistry != null)
{
    mcpBuilder
        .WithListToolsHandler(customToolRegistry.HandleListToolsAsync)
        .WithCallToolHandler(customToolRegistry.HandleCallToolAsync);
}

await builder.Build().RunAsync();
