using UniCortex.Mcp.Domains.Interfaces;
using ConsoleToolsClass = UniCortex.Mcp.Tools.ConsoleTools;

namespace UniCortex.Mcp.Test.Fixtures;

public sealed class ConsoleToolsFixture : ToolFixtureBase
{
    public ConsoleToolsClass ConsoleTools { get; }

    private ConsoleToolsFixture(string baseUrl, IHttpClientFactory httpClientFactory,
        IUnityServerUrlProvider urlProvider)
        : base(baseUrl, httpClientFactory, urlProvider)
    {
        ConsoleTools = CreateTool<ConsoleToolsClass>();
    }

    public static async ValueTask<ConsoleToolsFixture> CreateAsync()
    {
        var (baseUrl, httpClientFactory, urlProvider) = await InitializeAsync();
        return new ConsoleToolsFixture(baseUrl, httpClientFactory, urlProvider);
    }
}
