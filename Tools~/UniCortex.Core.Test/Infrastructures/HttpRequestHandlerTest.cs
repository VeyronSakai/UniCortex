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
    public async ValueTask SendAsync_RetriesAfterDisconnect()
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
    public async ValueTask SendAsync_RetriesAfterEmptyResponse()
    {
        // Arrange
        var inner = new ScriptedHandler(new HttpResponseMessage(HttpStatusCode.OK), Ok());
        using var client = CreateClient(inner);

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public async ValueTask SendAsync_RetriesRefusedConnection()
    {
        // Arrange
        var refused = new HttpRequestException("refused", new SocketException((int)SocketError.ConnectionRefused));
        var inner = new ScriptedHandler(refused, Ok());
        using var client = CreateClient(inner);

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public async ValueTask SendAsync_RetriesRequestCancelledByServerStop()
    {
        // Arrange
        var inner = new ScriptedHandler(ServiceUnavailable(), Ok());
        using var client = CreateClient(inner);

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    private static HttpClient CreateClient(HttpMessageHandler inner)
    {
        return new HttpClient(new HttpRequestHandler(NullLogger<HttpRequestHandler>.Instance) { InnerHandler = inner });
    }

    private static HttpResponseMessage Ok()
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
    }

    private static HttpResponseMessage ServiceUnavailable()
    {
        return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent("{\"error\":\"The server was stopped.\"}")
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
