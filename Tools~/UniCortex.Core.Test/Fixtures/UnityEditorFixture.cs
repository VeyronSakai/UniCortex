using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.Fixtures;

public sealed class UnityEditorFixture
{
    public EditorUseCase EditorUseCase { get; }
    public TestUseCase TestUseCase { get; }
    public ConsoleUseCase ConsoleUseCase { get; }
    public SceneUseCase SceneUseCase { get; }
    public GameObjectUseCase GameObjectUseCase { get; }
    public ComponentUseCase ComponentUseCase { get; }
    public PrefabUseCase PrefabUseCase { get; }
    public AssetUseCase AssetUseCase { get; }
    public MenuItemUseCase MenuItemUseCase { get; }
    public ScreenshotUseCase ScreenshotUseCase { get; }
    public ViewUseCase ViewUseCase { get; }

    public InputUseCase InputUseCase { get; }
    public TimelineUseCase TimelineUseCase { get; }
    public string BaseUrl { get; }

    private UnityEditorFixture(ServiceProvider provider, string baseUrl)
    {
        EditorUseCase = provider.GetRequiredService<EditorUseCase>();
        TestUseCase = provider.GetRequiredService<TestUseCase>();
        ConsoleUseCase = provider.GetRequiredService<ConsoleUseCase>();
        SceneUseCase = provider.GetRequiredService<SceneUseCase>();
        GameObjectUseCase = provider.GetRequiredService<GameObjectUseCase>();
        ComponentUseCase = provider.GetRequiredService<ComponentUseCase>();
        PrefabUseCase = provider.GetRequiredService<PrefabUseCase>();
        AssetUseCase = provider.GetRequiredService<AssetUseCase>();
        MenuItemUseCase = provider.GetRequiredService<MenuItemUseCase>();
        ScreenshotUseCase = provider.GetRequiredService<ScreenshotUseCase>();
        ViewUseCase = provider.GetRequiredService<ViewUseCase>();

        InputUseCase = provider.GetRequiredService<InputUseCase>();
        TimelineUseCase = provider.GetRequiredService<TimelineUseCase>();
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
