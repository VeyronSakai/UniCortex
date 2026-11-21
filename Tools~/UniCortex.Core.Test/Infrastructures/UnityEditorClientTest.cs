using System.Net.Http;
using NUnit.Framework;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Infrastructures;

namespace UniCortex.Core.Test.Infrastructures;

[TestFixture]
public class UnityEditorClientTest
{
    [Test]
    public void WaitForServerAsync_EditorDoesNotRespondInTime_ThrowsTimeoutWithHint()
    {
        // Arrange
        using var httpClient = new HttpClient(new NeverAnsweringHandler()) { Timeout = TimeSpan.FromMilliseconds(100) };
        var client = new UnityEditorClient(new StubHttpClientFactory(httpClient), new StubUrlProvider());

        // Act
        var exception = Assert.ThrowsAsync<TimeoutException>(async () => await client.WaitForServerAsync());

        // Assert
        Assert.That(exception!.Message, Does.StartWith("The Unity Editor did not respond within"));
        Assert.That(exception.Message, Does.Contain("get_editor_dialogs"));
        Assert.That(exception.InnerException, Is.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public void WaitForServerAsync_CancelledByCaller_ThrowsCancellation()
    {
        // Arrange
        using var httpClient = new HttpClient(new NeverAnsweringHandler()) { Timeout = TimeSpan.FromMinutes(1) };
        var client = new UnityEditorClient(new StubHttpClientFactory(httpClient), new StubUrlProvider());
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        Assert.CatchAsync<OperationCanceledException>(async () => await client.WaitForServerAsync(cts.Token));
    }

    private sealed class NeverAnsweringHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("Not reached.");
        }
    }

    private sealed class StubHttpClientFactory(HttpClient httpClient) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return httpClient;
        }
    }

    private sealed class StubUrlProvider : IUnityServerUrlProvider
    {
        public string GetUrl()
        {
            return "http://localhost:1";
        }
    }
}
