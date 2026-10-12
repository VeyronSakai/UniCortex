using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Domains.Models;
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

    [Test]
    public async ValueTask SendAsync_RetriesResponseWrittenByClosingListener()
    {
        // Arrange
        var listenerResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("<h1>Bad Request (Invalid host)</h1>", Encoding.UTF8, "text/html")
        };
        var inner = new ScriptedHandler(listenerResponse, Ok());
        using var client = CreateClient(inner);

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public async ValueTask SendAsync_DoesNotRetryErrorWrittenByHandler()
    {
        // Arrange
        var handlerError = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = Json("{\"error\":\"Script compilation failed\"}")
        };
        var inner = new ScriptedHandler(handlerError);
        using var client = CreateClient(inner);

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }

    [Test]
    public void SendAsync_MainThreadNotRespondingWithDialog_FailsWithDialogContents()
    {
        // Arrange
        var inner = new ScriptedHandler(GatewayTimeout(), Ok());
        var dialog = new EditorDialog
        {
            title = "Saving failed",
            message = "Could not save the scene.",
            buttons = ["Try Again", "Cancel"]
        };
        using var client = CreateClient(inner, new FakeEditorDialogAccessor(dialog));

        // Act
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/")));

        // Assert
        Assert.That(exception!.Message, Does.Contain("Saving failed"));
        Assert.That(exception.Message, Does.Contain("Could not save the scene."));
        Assert.That(exception.Message, Does.Contain("\"Try Again\", \"Cancel\""));
        Assert.That(exception.Message, Does.Contain("click_editor_dialog_button"));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }

    [Test]
    public async ValueTask SendAsync_MainThreadNotRespondingWithoutDialog_Retries()
    {
        // Arrange
        var inner = new ScriptedHandler(GatewayTimeout(), Ok());
        using var client = CreateClient(inner, new FakeEditorDialogAccessor());

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public async ValueTask SendAsync_MainThreadNotRespondingAndDialogsUnreadable_Retries()
    {
        // Arrange
        var inner = new ScriptedHandler(GatewayTimeout(), Ok());
        using var client = CreateClient(inner, new FakeEditorDialogAccessor { ThrowsOnRead = true });

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    private static HttpClient CreateClient(HttpMessageHandler inner, IEditorDialogAccessor? dialogAccessor = null)
    {
        var handler = new HttpRequestHandler(NullLogger<HttpRequestHandler>.Instance, new FakeProcessProvider(),
            dialogAccessor ?? new FakeEditorDialogAccessor())
        {
            InnerHandler = inner
        };
        return new HttpClient(handler);
    }

    private static HttpResponseMessage Ok()
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = Json("{}") };
    }

    private static HttpResponseMessage ServiceUnavailable()
    {
        return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            Content = Json("{\"error\":\"The server was stopped.\"}")
        };
    }

    private static HttpResponseMessage GatewayTimeout()
    {
        return new HttpResponseMessage(HttpStatusCode.GatewayTimeout)
        {
            Content = Json("{\"error\":\"The Unity Editor's main thread has not responded.\"}")
        };
    }

    private static StringContent Json(string json)
    {
        return new StringContent(json, Encoding.UTF8, "application/json");
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

    private sealed class FakeProcessProvider : IUnityEditorProcessProvider
    {
        public int GetProcessId()
        {
            return 1234;
        }
    }

    private sealed class FakeEditorDialogAccessor(params EditorDialog[] dialogs) : IEditorDialogAccessor
    {
        public bool ThrowsOnRead { get; init; }

        public IReadOnlyList<EditorDialog> GetDialogs(int processId)
        {
            if (ThrowsOnRead)
            {
                throw new PlatformNotSupportedException("Not supported.");
            }

            return dialogs;
        }

        public EditorDialog? ClickButton(int processId, string buttonLabel)
        {
            throw new NotSupportedException();
        }
    }
}
