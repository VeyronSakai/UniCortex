using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniCortex.Core.Domains;
using UniCortex.Core.Extensions;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;
using AssetToolsClass = UniCortex.Mcp.Tools.AssetTools;
using ComponentToolsClass = UniCortex.Mcp.Tools.ComponentTools;
using ConsoleToolsClass = UniCortex.Mcp.Tools.ConsoleTools;
using EditorToolsClass = UniCortex.Mcp.Tools.EditorTools;
using GameObjectToolsClass = UniCortex.Mcp.Tools.GameObjectTools;
using PrefabToolsClass = UniCortex.Mcp.Tools.PrefabTools;
using SceneToolsClass = UniCortex.Mcp.Tools.SceneTools;
using TestToolsClass = UniCortex.Mcp.Tools.TestTools;
using MenuItemToolsClass = UniCortex.Mcp.Tools.MenuItemTools;
using ScreenshotToolsClass = UniCortex.Mcp.Tools.ScreenshotTools;

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
    public MenuItemToolsClass MenuItemTools { get; }
    public ScreenshotToolsClass ScreenshotTools { get; }
    public string BaseUrl { get; }

    private UnityEditorFixture(EditorToolsClass editorTools, TestToolsClass testTools,
        ConsoleToolsClass consoleTools, SceneToolsClass sceneTools, GameObjectToolsClass gameObjectTools,
        ComponentToolsClass componentTools, PrefabToolsClass prefabTools, AssetToolsClass assetTools,
        MenuItemToolsClass menuItemTools, ScreenshotToolsClass screenshotTools, string baseUrl)
    {
        EditorTools = editorTools;
        TestTools = testTools;
        ConsoleTools = consoleTools;
        SceneTools = sceneTools;
        GameObjectTools = gameObjectTools;
        ComponentTools = componentTools;
        PrefabTools = prefabTools;
        AssetTools = assetTools;
        MenuItemTools = menuItemTools;
        ScreenshotTools = screenshotTools;
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
        // Build DI container
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddUniCortexCore();

        var provider = services.BuildServiceProvider();
        var urlProvider = provider.GetRequiredService<Core.Domains.Interfaces.IUnityServerUrlProvider>();
        var baseUrl = urlProvider.GetUrl();

        // Connection check using the retry-capable HttpClient.
        // HttpRequestHandler automatically retries during domain reloads.
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var checkClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);
        var pingResponse = await checkClient.GetAsync($"{baseUrl}{ApiRoutes.Ping}");
        pingResponse.EnsureSuccessStatusCode();

        var editorTools = new EditorToolsClass(provider.GetRequiredService<EditorUseCase>());
        var testTools = new TestToolsClass(provider.GetRequiredService<TestUseCase>());
        var consoleTools = new ConsoleToolsClass(provider.GetRequiredService<ConsoleUseCase>());
        var sceneTools = new SceneToolsClass(provider.GetRequiredService<SceneUseCase>());
        var gameObjectTools = new GameObjectToolsClass(provider.GetRequiredService<GameObjectUseCase>());
        var componentTools = new ComponentToolsClass(provider.GetRequiredService<ComponentUseCase>());
        var prefabTools = new PrefabToolsClass(provider.GetRequiredService<PrefabUseCase>());
        var assetTools = new AssetToolsClass(provider.GetRequiredService<AssetUseCase>());
        var menuItemTools = new MenuItemToolsClass(provider.GetRequiredService<MenuItemUseCase>());
        var screenshotTools = new ScreenshotToolsClass(provider.GetRequiredService<ScreenshotUseCase>());

        return new UnityEditorFixture(editorTools, testTools, consoleTools, sceneTools, gameObjectTools,
            componentTools, prefabTools, assetTools, menuItemTools, screenshotTools, baseUrl);
    }

    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };

    /// <summary>
    /// Ensures the editor is not in play mode before performing scene operations.
    /// </summary>
    public async ValueTask EnsureNotInPlayModeAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        for (var attempt = 0; attempt < 60; attempt++)
        {
            try
            {
                var statusResponse = await client.GetAsync($"{BaseUrl}{ApiRoutes.Status}", cancellationToken);
                var json = await statusResponse.Content.ReadAsStringAsync(cancellationToken);
                var status = JsonSerializer.Deserialize<EditorStatusResponse>(json, s_jsonOptions);
                if (status is not { isPlaying: true })
                {
                    return;
                }

                // In play mode — request stop and poll
                await client.PostAsync($"{BaseUrl}{ApiRoutes.Stop}", null, cancellationToken);
                await Task.Delay(500, cancellationToken);
            }
            catch
            {
                // Server may be reloading domain; retry after delay
                await Task.Delay(500, cancellationToken);
            }
        }
    }
}
