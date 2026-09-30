using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using UniCortex.Core.Infrastructures;

namespace UniCortex.Core.Test.Infrastructures;

[TestFixture]
public class HttpRequestHandlerTest
{
    [Test]
    public async ValueTask SendAsync_RetriesAfterDisconnect_ByDefault()
    {
        // Arrange
        var inner = new ScriptedHandler(new HttpRequestException(), Ok());
        using var client = CreateClient(inner);

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public void SendAsync_DoesNotResendAfterDisconnect_WhenDisabled()
    {
        // Arrange
        var inner = new ScriptedHandler(new HttpRequestException(), Ok());
        using var client = CreateClient(inner);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => await client.SendAsync(CreateNoResendRequest()));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }

    [Test]
    public void SendAsync_DoesNotResendAfterEmptyResponse_WhenDisabled()
    {
        // Arrange
        var inner = new ScriptedHandler(new HttpResponseMessage(HttpStatusCode.OK), Ok());
        using var client = CreateClient(inner);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => await client.SendAsync(CreateNoResendRequest()));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }

    [Test]
    public async ValueTask SendAsync_RetriesRefusedConnection_WhenResendIsDisabled()
    {
        // Arrange
        var refused = new HttpRequestException("refused", new SocketException((int)SocketError.ConnectionRefused));
        var inner = new ScriptedHandler(refused, Ok());
        using var client = CreateClient(inner);

        // Act
        using var response = await client.SendAsync(CreateNoResendRequest());

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public async ValueTask SendAsync_RetriesRequestCancelledByServerStop_ByDefault()
    {
        // Arrange
        var inner = new ScriptedHandler(RequestTimeout(), Ok());
        using var client = CreateClient(inner);

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public async ValueTask SendAsync_ReturnsRequestCancelledByServerStop_WhenResendIsDisabled()
    {
        // Arrange
        var inner = new ScriptedHandler(RequestTimeout(), Ok());
        using var client = CreateClient(inner);

        // Act
        using var response = await client.SendAsync(CreateNoResendRequest());

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.RequestTimeout));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }

    private static HttpClient CreateClient(HttpMessageHandler inner)
    {
        return new HttpClient(new HttpRequestHandler(NullLogger<HttpRequestHandler>.Instance) { InnerHandler = inner });
    }

    private static HttpRequestMessage CreateNoResendRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/");
        request.Options.Set(HttpRequestHandler.ResendAfterDisconnectKey, false);
        return request;
    }

    private static HttpResponseMessage Ok()
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
    }

    private static HttpResponseMessage RequestTimeout()
    {
        return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
        {
            Content = new StringContent("{\"error\":\"Request was cancelled.\"}")
        };
    }

    // Returns (or throws) the scripted outcomes in order, one per call.
    private sealed class ScriptedHandler(params object[] outcomes) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var outcome = outcomes[CallCount++];
            return outcome is Exception exception
                ? Task.FromException<HttpResponseMessage>(exception)
                : Task.FromResult((HttpResponseMessage)outcome);
        }
    }
}
