using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Infrastructures;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddTransient<HttpRequestHandler>();

// 優先度1: UNICORTEX_URL 環境変数
var baseUrl = Environment.GetEnvironmentVariable("UNICORTEX_URL");

// 優先度2: UNICORTEX_PROJECT_PATH 環境変数
if (baseUrl is null)
{
    var projectPath = Environment.GetEnvironmentVariable("UNICORTEX_PROJECT_PATH");
    if (projectPath is not null)
    {
        var path = Path.Combine(projectPath, "Library", "UniCortex", "config.json");
        if (File.Exists(path))
        {
            var options = new JsonSerializerOptions { IncludeFields = true };
            var config = JsonSerializer.Deserialize<UnityServerConfig>(File.ReadAllText(path), options);
            if (!string.IsNullOrEmpty(config?.server_url))
                baseUrl = config.server_url;
        }
    }
}

// どちらもなければエラー
if (baseUrl is null)
{
    Console.Error.WriteLine(
        "Could not determine the UniCortex server URL. " +
        "Set UNICORTEX_PROJECT_PATH to your Unity project path, " +
        "or set UNICORTEX_URL to the server URL directly.");
    Environment.Exit(1);
    return;
}

builder.Services.AddHttpClient("UniCortex", client =>
{
    if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
    {
        Console.Error.WriteLine($"Invalid URL '{baseUrl}'.");
        Environment.Exit(1);
        return;
    }

    client.BaseAddress = baseUri;
}).AddHttpMessageHandler<HttpRequestHandler>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
