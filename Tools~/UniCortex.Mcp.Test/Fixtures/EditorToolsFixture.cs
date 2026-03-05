using UniCortex.Mcp.Domains.Interfaces;
using EditorToolsClass = UniCortex.Mcp.Tools.EditorTools;

namespace UniCortex.Mcp.Test.Fixtures;

public sealed class EditorToolsFixture : ToolFixtureBase
{
    public EditorToolsClass EditorTools { get; }

    private EditorToolsFixture(string baseUrl, IHttpClientFactory httpClientFactory,
        IUnityServerUrlProvider urlProvider)
        : base(baseUrl, httpClientFactory, urlProvider)
    {
        EditorTools = CreateTool<EditorToolsClass>();
    }

    public static async ValueTask<EditorToolsFixture> CreateAsync()
    {
        var (baseUrl, httpClientFactory, urlProvider) = await InitializeAsync();
        return new EditorToolsFixture(baseUrl, httpClientFactory, urlProvider);
    }
}
