using UniCortex.Mcp.Domains.Interfaces;
using MenuItemToolsClass = UniCortex.Mcp.Tools.MenuItemTools;

namespace UniCortex.Mcp.Test.Fixtures;

public sealed class MenuItemToolsFixture : ToolFixtureBase
{
    public MenuItemToolsClass MenuItemTools { get; }

    private MenuItemToolsFixture(string baseUrl, IHttpClientFactory httpClientFactory,
        IUnityServerUrlProvider urlProvider)
        : base(baseUrl, httpClientFactory, urlProvider)
    {
        MenuItemTools = CreateTool<MenuItemToolsClass>();
    }

    public static async ValueTask<MenuItemToolsFixture> CreateAsync()
    {
        var (baseUrl, httpClientFactory, urlProvider) = await InitializeAsync();
        return new MenuItemToolsFixture(baseUrl, httpClientFactory, urlProvider);
    }
}
