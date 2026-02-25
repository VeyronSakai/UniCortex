using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UniCortex.Mcp.Domains.Interfaces;
using UniCortex.Mcp.Infrastructures;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddTransient<HttpRequestHandler>();
builder.Services.AddTransient<IUnityServerUrlProvider, UnityServerUrlProvider>();
builder.Services.AddHttpClient("UniCortex", client =>
    {
        // Test runs can take several minutes, so increase the default timeout.
        client.Timeout = TimeSpan.FromMinutes(10);
    })
    .AddHttpMessageHandler<HttpRequestHandler>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
