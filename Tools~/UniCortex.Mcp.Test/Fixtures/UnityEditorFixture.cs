using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Infrastructures;
using EditorToolsClass = UniCortex.Mcp.Tools.EditorTools;

namespace UniCortex.Mcp.Test.Fixtures;

public sealed class UnityEditorFixture
{
    public EditorToolsClass EditorTools { get; }
    public string BaseUrl { get; }

    private UnityEditorFixture(EditorToolsClass editorTools, string baseUrl)
    {
        EditorTools = editorTools;
        BaseUrl = baseUrl;
    }

    public static async ValueTask<UnityEditorFixture> CreateAsync()
    {
        var urlProvider = new UnityServerUrlProvider();

        string baseUrl;
        try
        {
            baseUrl = urlProvider.GetUrl();
        }
        catch (InvalidOperationException ex)
        {
            Assert.Ignore($"Skipping: {ex.Message}");
            return null!; // unreachable
        }

        // Connection check with a plain HttpClient (no retry handler)
        using var checkClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        try
        {
            var response = await checkClient.GetAsync($"{baseUrl}{ApiRoutes.Ping}");
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            Assert.Ignore($"Skipping: Unity Editor is not reachable at {baseUrl}.");
            return null!; // unreachable
        }

        // Build DI container
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddTransient<HttpRequestHandler>();
        services.AddHttpClient("UniCortex")
            .AddHttpMessageHandler<HttpRequestHandler>();

        var provider = services.BuildServiceProvider();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var editorTools = new EditorToolsClass(httpClientFactory, urlProvider);

        return new UnityEditorFixture(editorTools, baseUrl);
    }
}
