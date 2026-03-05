using UniCortex.Mcp.Domains.Interfaces;
using ScreenshotToolsClass = UniCortex.Mcp.Tools.ScreenshotTools;
using EditorToolsClass = UniCortex.Mcp.Tools.EditorTools;

namespace UniCortex.Mcp.Test.Fixtures;

public sealed class ScreenshotToolsFixture : ToolFixtureBase
{
    public ScreenshotToolsClass ScreenshotTools { get; }
    public EditorToolsClass EditorTools { get; }

    private ScreenshotToolsFixture(string baseUrl, IHttpClientFactory httpClientFactory,
        IUnityServerUrlProvider urlProvider)
        : base(baseUrl, httpClientFactory, urlProvider)
    {
        ScreenshotTools = CreateTool<ScreenshotToolsClass>();
        EditorTools = CreateTool<EditorToolsClass>();
    }

    public static async ValueTask<ScreenshotToolsFixture> CreateAsync()
    {
        var (baseUrl, httpClientFactory, urlProvider) = await InitializeAsync();
        return new ScreenshotToolsFixture(baseUrl, httpClientFactory, urlProvider);
    }
}
