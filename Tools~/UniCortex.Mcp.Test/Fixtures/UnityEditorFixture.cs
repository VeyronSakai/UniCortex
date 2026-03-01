using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Infrastructures;
using AssetToolsClass = UniCortex.Mcp.Tools.AssetTools;
using ComponentToolsClass = UniCortex.Mcp.Tools.ComponentTools;
using ConsoleToolsClass = UniCortex.Mcp.Tools.ConsoleTools;
using EditorToolsClass = UniCortex.Mcp.Tools.EditorTools;
using GameObjectToolsClass = UniCortex.Mcp.Tools.GameObjectTools;
using PrefabToolsClass = UniCortex.Mcp.Tools.PrefabTools;
using SceneToolsClass = UniCortex.Mcp.Tools.SceneTools;
using TestToolsClass = UniCortex.Mcp.Tools.TestTools;
using UtilityToolsClass = UniCortex.Mcp.Tools.UtilityTools;

namespace UniCortex.Mcp.Test.Fixtures;

public sealed class UnityEditorFixture
{
    public EditorToolsClass EditorTools { get; }
    public TestToolsClass TestTools { get; }
    public ConsoleToolsClass ConsoleTools { get; }
    public SceneToolsClass SceneTools { get; }
    public GameObjectToolsClass GameObjectTools { get; }
    public ComponentToolsClass ComponentTools { get; }
    public PrefabToolsClass PrefabTools { get; }
    public AssetToolsClass AssetTools { get; }
    public UtilityToolsClass UtilityTools { get; }
    public string BaseUrl { get; }

    private UnityEditorFixture(EditorToolsClass editorTools, TestToolsClass testTools,
        ConsoleToolsClass consoleTools, SceneToolsClass sceneTools, GameObjectToolsClass gameObjectTools,
        ComponentToolsClass componentTools, PrefabToolsClass prefabTools, AssetToolsClass assetTools,
        UtilityToolsClass utilityTools, string baseUrl)
    {
        EditorTools = editorTools;
        TestTools = testTools;
        ConsoleTools = consoleTools;
        SceneTools = sceneTools;
        GameObjectTools = gameObjectTools;
        ComponentTools = componentTools;
        PrefabTools = prefabTools;
        AssetTools = assetTools;
        UtilityTools = utilityTools;
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
        var consoleTools = new ConsoleToolsClass(httpClientFactory, urlProvider);
        var sceneTools = new SceneToolsClass(httpClientFactory, urlProvider);
        var gameObjectTools = new GameObjectToolsClass(httpClientFactory, urlProvider);
        var componentTools = new ComponentToolsClass(httpClientFactory, urlProvider);
        var prefabTools = new PrefabToolsClass(httpClientFactory, urlProvider);
        var assetTools = new AssetToolsClass(httpClientFactory, urlProvider);
        var utilityTools = new UtilityToolsClass(httpClientFactory, urlProvider);

        return new UnityEditorFixture(editorTools, testTools, consoleTools, sceneTools, gameObjectTools,
            componentTools, prefabTools, assetTools, utilityTools, baseUrl);
    }
}
