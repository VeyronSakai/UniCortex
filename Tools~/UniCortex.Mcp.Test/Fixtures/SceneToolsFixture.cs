using UniCortex.Mcp.Domains.Interfaces;
using SceneToolsClass = UniCortex.Mcp.Tools.SceneTools;

namespace UniCortex.Mcp.Test.Fixtures;

public sealed class SceneToolsFixture : ToolFixtureBase
{
    public SceneToolsClass SceneTools { get; }

    private SceneToolsFixture(string baseUrl, IHttpClientFactory httpClientFactory,
        IUnityServerUrlProvider urlProvider)
        : base(baseUrl, httpClientFactory, urlProvider)
    {
        SceneTools = CreateTool<SceneToolsClass>();
    }

    public static async ValueTask<SceneToolsFixture> CreateAsync()
    {
        var (baseUrl, httpClientFactory, urlProvider) = await InitializeAsync();
        return new SceneToolsFixture(baseUrl, httpClientFactory, urlProvider);
    }
}
