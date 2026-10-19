using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Domains.Models;
using UniCortex.Core.Infrastructures;

namespace UniCortex.Core.Test.Infrastructures;

[TestFixture]
public class EditorDialogWatchHandlerTest
{
    [Test]
    public void SendAsync_DialogOpenWhileWaiting_FailsWithDialogContentsAndCancelsRequest()
    {
        // Arrange
        var inner = new DelayedHandler(TimeSpan.FromMinutes(1));
        var dialog = new EditorDialog
        {
            title = "Saving failed",
            message = "Could not save the scene.",
            buttons = ["Try Again", "Cancel"]
        };
        var accessor = new FakeEditorDialogAccessor(dialog);
        using var client = CreateClient(inner, accessor);

        // Act
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/")));

        // Assert
        Assert.That(exception!.Message, Does.Contain("Saving failed"));
        Assert.That(exception.Message, Does.Contain("Could not save the scene."));
        Assert.That(exception.Message, Does.Contain("\"Try Again\", \"Cancel\""));
        Assert.That(exception.Message, Does.Contain("click_editor_dialog_button"));
        Assert.That(inner.WasCancelled, Is.True);
    }

    [Test]
    public async ValueTask SendAsync_NoDialogWhileWaiting_ReturnsResponse()
    {
        // Arrange
        var inner = new DelayedHandler(TimeSpan.FromMilliseconds(300));
        var accessor = new FakeEditorDialogAccessor();
        using var client = CreateClient(inner, accessor);

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(accessor.ReadCount, Is.GreaterThan(0));
    }

    [Test]
    public async ValueTask SendAsync_DialogsUnreadable_ReturnsResponse()
    {
        // Arrange
        var inner = new DelayedHandler(TimeSpan.FromMilliseconds(300));
        var accessor = new FakeEditorDialogAccessor { ThrowsOnRead = true };
        using var client = CreateClient(inner, accessor);

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(accessor.ReadCount, Is.GreaterThan(0));
    }

    [Test]
    public async ValueTask SendAsync_RespondsBeforeFirstCheck_DoesNotReadDialogs()
    {
        // Arrange
        var inner = new DelayedHandler(TimeSpan.Zero);
        var accessor = new FakeEditorDialogAccessor();
        using var client = CreateClient(inner, accessor, firstCheckDelay: TimeSpan.FromSeconds(10));

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(accessor.ReadCount, Is.EqualTo(0));
    }

    private static HttpClient CreateClient(HttpMessageHandler inner, IEditorDialogAccessor accessor,
        TimeSpan? firstCheckDelay = null)
    {
        var handler = new EditorDialogWatchHandler(NullLogger<EditorDialogWatchHandler>.Instance,
            new FakeProcessProvider(), accessor)
        {
            FirstCheckDelay = firstCheckDelay ?? TimeSpan.FromMilliseconds(50),
            CheckInterval = TimeSpan.FromMilliseconds(20),
            InnerHandler = inner
        };
        return new HttpClient(handler);
    }

    // Answers 200 after the delay, unless the request is cancelled first.
    private sealed class DelayedHandler(TimeSpan delay) : HttpMessageHandler
    {
        public bool WasCancelled { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Registered callbacks run synchronously in Cancel(), before the handler throws.
            await using var registration = cancellationToken.Register(() => WasCancelled = true);
            await Task.Delay(delay, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
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
        private int _readCount;

        public bool ThrowsOnRead { get; init; }
        public int ReadCount => Volatile.Read(ref _readCount);

        public IReadOnlyList<EditorDialog> GetDialogs(int processId)
        {
            Interlocked.Increment(ref _readCount);
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
