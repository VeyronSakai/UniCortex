using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Infrastructures;
using EditorToolsClass = UniCortex.Mcp.Tools.EditorTools;
using TestToolsClass = UniCortex.Mcp.Tools.TestTools;

namespace UniCortex.Mcp.Test.Fixtures;

public sealed class UnityEditorFixture
{
    public EditorToolsClass EditorTools { get; }
    public TestToolsClass TestTools { get; }
    public string BaseUrl { get; }

    private UnityEditorFixture(EditorToolsClass editorTools, TestToolsClass testTools, string baseUrl)
    {
        EditorTools = editorTools;
        TestTools = testTools;
        BaseUrl = baseUrl;
    }

    public static async ValueTask<UnityEditorFixture> CreateAsync()
    {
        var urlProvider = new UnityServerUrlProvider();
        var baseUrl = urlProvider.GetUrl();

        // Connection check with a plain HttpClient (no retry handler)
        using var checkClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        var response = await checkClient.GetAsync($"{baseUrl}{ApiRoutes.Ping}");
        response.EnsureSuccessStatusCode();

        // Build DI container
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddTransient<HttpRequestHandler>();
        services.AddHttpClient("UniCortex")
            .AddHttpMessageHandler<HttpRequestHandler>();

        var provider = services.BuildServiceProvider();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var editorTools = new EditorToolsClass(httpClientFactory, urlProvider);
        var testTools = new TestToolsClass(httpClientFactory, urlProvider);

        return new UnityEditorFixture(editorTools, testTools, baseUrl);
    }
}
