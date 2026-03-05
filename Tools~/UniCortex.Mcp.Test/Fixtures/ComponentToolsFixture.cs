using UniCortex.Mcp.Domains.Interfaces;
using ComponentToolsClass = UniCortex.Mcp.Tools.ComponentTools;
using GameObjectToolsClass = UniCortex.Mcp.Tools.GameObjectTools;

namespace UniCortex.Mcp.Test.Fixtures;

public sealed class ComponentToolsFixture : ToolFixtureBase
{
    public ComponentToolsClass ComponentTools { get; }
    public GameObjectToolsClass GameObjectTools { get; }

    private ComponentToolsFixture(string baseUrl, IHttpClientFactory httpClientFactory,
        IUnityServerUrlProvider urlProvider)
        : base(baseUrl, httpClientFactory, urlProvider)
    {
        ComponentTools = CreateTool<ComponentToolsClass>();
        GameObjectTools = CreateTool<GameObjectToolsClass>();
    }

    public static async ValueTask<ComponentToolsFixture> CreateAsync()
    {
        var (baseUrl, httpClientFactory, urlProvider) = await InitializeAsync();
        return new ComponentToolsFixture(baseUrl, httpClientFactory, urlProvider);
    }
}
