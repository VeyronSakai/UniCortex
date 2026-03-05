using UniCortex.Mcp.Domains.Interfaces;
using AssetToolsClass = UniCortex.Mcp.Tools.AssetTools;

namespace UniCortex.Mcp.Test.Fixtures;

public sealed class AssetToolsFixture : ToolFixtureBase
{
    public AssetToolsClass AssetTools { get; }

    private AssetToolsFixture(string baseUrl, IHttpClientFactory httpClientFactory,
        IUnityServerUrlProvider urlProvider)
        : base(baseUrl, httpClientFactory, urlProvider)
    {
        AssetTools = CreateTool<AssetToolsClass>();
    }

    public static async ValueTask<AssetToolsFixture> CreateAsync()
    {
        var (baseUrl, httpClientFactory, urlProvider) = await InitializeAsync();
        return new AssetToolsFixture(baseUrl, httpClientFactory, urlProvider);
    }
}
