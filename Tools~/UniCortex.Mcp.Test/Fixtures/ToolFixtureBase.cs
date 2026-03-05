using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Domains.Interfaces;
using UniCortex.Mcp.Infrastructures;

namespace UniCortex.Mcp.Test.Fixtures;

public abstract class ToolFixtureBase
{
    public string BaseUrl { get; }

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUnityServerUrlProvider _urlProvider;

    protected ToolFixtureBase(string baseUrl, IHttpClientFactory httpClientFactory,
        IUnityServerUrlProvider urlProvider)
    {
        BaseUrl = baseUrl;
        _httpClientFactory = httpClientFactory;
        _urlProvider = urlProvider;
    }

    protected T CreateTool<T>() where T : class
    {
        return (T)Activator.CreateInstance(typeof(T), _httpClientFactory, _urlProvider)!;
    }

    /// <summary>
    /// Deletes an asset file and its .meta from disk using UNICORTEX_PROJECT_PATH.
    /// </summary>
    public static void DeleteAssetFile(string assetPath)
    {
        var projectPath = Environment.GetEnvironmentVariable("UNICORTEX_PROJECT_PATH");
        if (projectPath is null)
        {
            return;
        }

        var fullPath = Path.Combine(projectPath, assetPath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        var metaPath = fullPath + ".meta";
        if (File.Exists(metaPath))
        {
            File.Delete(metaPath);
        }
    }

    protected static async ValueTask<(string baseUrl, IHttpClientFactory httpClientFactory,
        IUnityServerUrlProvider urlProvider)> InitializeAsync()
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

        return (baseUrl, httpClientFactory, urlProvider);
    }
}
