using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Core.Services;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Test.Fixtures;

public sealed class UnityEditorFixture
{
    public EditorService EditorService { get; }
    public TestService TestService { get; }
    public ConsoleService ConsoleService { get; }
    public SceneService SceneService { get; }
    public GameObjectService GameObjectService { get; }
    public ComponentService ComponentService { get; }
    public PrefabService PrefabService { get; }
    public AssetService AssetService { get; }
    public MenuItemService MenuItemService { get; }
    public ScreenshotService ScreenshotService { get; }
    public string BaseUrl { get; }

    private UnityEditorFixture(ServiceProvider provider, string baseUrl)
    {
        EditorService = provider.GetRequiredService<EditorService>();
        TestService = provider.GetRequiredService<TestService>();
        ConsoleService = provider.GetRequiredService<ConsoleService>();
        SceneService = provider.GetRequiredService<SceneService>();
        GameObjectService = provider.GetRequiredService<GameObjectService>();
        ComponentService = provider.GetRequiredService<ComponentService>();
        PrefabService = provider.GetRequiredService<PrefabService>();
        AssetService = provider.GetRequiredService<AssetService>();
        MenuItemService = provider.GetRequiredService<MenuItemService>();
        ScreenshotService = provider.GetRequiredService<ScreenshotService>();
        BaseUrl = baseUrl;
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

    public static async ValueTask<UnityEditorFixture> CreateAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddUniCortexCore();

        var provider = services.BuildServiceProvider();
        var urlProvider = provider.GetRequiredService<IUnityServerUrlProvider>();
        var baseUrl = urlProvider.GetUrl();

        // Connection check using the retry-capable HttpClient.
        // HttpRequestHandler automatically retries during domain reloads.
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var checkClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);
        var pingResponse = await checkClient.GetAsync($"{baseUrl}{ApiRoutes.Ping}");
        pingResponse.EnsureSuccessStatusCode();

        return new UnityEditorFixture(provider, baseUrl);
    }
}
