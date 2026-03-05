using UniCortex.Mcp.Domains.Interfaces;
using GameObjectToolsClass = UniCortex.Mcp.Tools.GameObjectTools;

namespace UniCortex.Mcp.Test.Fixtures;

public sealed class GameObjectToolsFixture : ToolFixtureBase
{
    public GameObjectToolsClass GameObjectTools { get; }

    private GameObjectToolsFixture(string baseUrl, IHttpClientFactory httpClientFactory,
        IUnityServerUrlProvider urlProvider)
        : base(baseUrl, httpClientFactory, urlProvider)
    {
        GameObjectTools = CreateTool<GameObjectToolsClass>();
    }

    public static async ValueTask<GameObjectToolsFixture> CreateAsync()
    {
        var (baseUrl, httpClientFactory, urlProvider) = await InitializeAsync();
        return new GameObjectToolsFixture(baseUrl, httpClientFactory, urlProvider);
    }
}
