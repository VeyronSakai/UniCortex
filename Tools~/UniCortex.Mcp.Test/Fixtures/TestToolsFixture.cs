using UniCortex.Mcp.Domains.Interfaces;
using TestToolsClass = UniCortex.Mcp.Tools.TestTools;

namespace UniCortex.Mcp.Test.Fixtures;

public sealed class TestToolsFixture : ToolFixtureBase
{
    public TestToolsClass TestTools { get; }

    private TestToolsFixture(string baseUrl, IHttpClientFactory httpClientFactory,
        IUnityServerUrlProvider urlProvider)
        : base(baseUrl, httpClientFactory, urlProvider)
    {
        TestTools = CreateTool<TestToolsClass>();
    }

    public static async ValueTask<TestToolsFixture> CreateAsync()
    {
        var (baseUrl, httpClientFactory, urlProvider) = await InitializeAsync();
        return new TestToolsFixture(baseUrl, httpClientFactory, urlProvider);
    }
}
