using UniCortex.Mcp.Domains.Interfaces;
using PrefabToolsClass = UniCortex.Mcp.Tools.PrefabTools;
using GameObjectToolsClass = UniCortex.Mcp.Tools.GameObjectTools;

namespace UniCortex.Mcp.Test.Fixtures;

public sealed class PrefabToolsFixture : ToolFixtureBase
{
    public PrefabToolsClass PrefabTools { get; }
    public GameObjectToolsClass GameObjectTools { get; }

    private PrefabToolsFixture(string baseUrl, IHttpClientFactory httpClientFactory,
        IUnityServerUrlProvider urlProvider)
        : base(baseUrl, httpClientFactory, urlProvider)
    {
        PrefabTools = CreateTool<PrefabToolsClass>();
        GameObjectTools = CreateTool<GameObjectToolsClass>();
    }

    public static async ValueTask<PrefabToolsFixture> CreateAsync()
    {
        var (baseUrl, httpClientFactory, urlProvider) = await InitializeAsync();
        return new PrefabToolsFixture(baseUrl, httpClientFactory, urlProvider);
    }
}
