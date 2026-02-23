using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniCortex.Mcp.Domains.Interfaces;
using UniCortex.Mcp.Infrastructures;
using UniCortex.Mcp.IntegrationTests.Fixtures;
using UniCortex.Mcp.Tools;

namespace UniCortex.Mcp.IntegrationTests;

[SetUpFixture]
public class UnityEditorFixture
{
    public static EditorTools EditorTools { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddTransient<HttpRequestHandler>();
        services.AddSingleton<IUnityServerUrlProvider, TestUrlProvider>();
        services.AddHttpClient("UniCortex", client =>
            {
                // Disable HttpClient's default 100-second timeout.
                // Per-request timeouts are controlled by CancellationTokenSource in each test.
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddHttpMessageHandler<HttpRequestHandler>();

        var provider = services.BuildServiceProvider();

        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var urlProvider = provider.GetRequiredService<IUnityServerUrlProvider>();
        EditorTools = new EditorTools(httpClientFactory, urlProvider);

        // Verify connectivity with a TCP connection check.
        // Unity's HttpListener may bind to IPv6 only, so we check the actual port reachability.
        try
        {
            var baseUrl = new Uri(urlProvider.GetUrl());
            using var tcpClient = new TcpClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await tcpClient.ConnectAsync(baseUrl.Host, baseUrl.Port, cts.Token);
        }
        catch
        {
            Assert.Ignore("Unity Editor is not running. Skipping integration tests.");
        }
    }
}
