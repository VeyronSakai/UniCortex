using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using UniCortex.Core.Domains;
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
    public void SendAsync_DialogWithoutReadButtons_FailsWithHintToBringEditorToFront()
    {
        // Arrange
        var inner = new DelayedHandler(TimeSpan.FromMinutes(1));
        var accessor = new FakeEditorDialogAccessor(new EditorDialog { title = "Save Scene", message = "Save Scene" });
        using var client = CreateClient(inner, accessor);

        // Act
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/")));

        // Assert
        Assert.That(exception!.Message, Does.Contain("Buttons: none could be read"));
        Assert.That(exception.Message, Does.Contain("click_editor_dialog_button does"));
        Assert.That(exception.Message, Does.Contain("\"Cancel\""));
    }

    [Test]
    public void SendAsync_DialogWithButtons_FailsWithoutHintToBringEditorToFront()
    {
        // Arrange
        var inner = new DelayedHandler(TimeSpan.FromMinutes(1));
        var accessor = new FakeEditorDialogAccessor(new EditorDialog { title = "Saving failed", buttons = ["OK"] });
        using var client = CreateClient(inner, accessor);

        // Act
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/")));

        // Assert
        Assert.That(exception!.Message, Does.Not.Contain("brought to the front"));
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
        var accessor = new FakeEditorDialogAccessor { ReadException = new PlatformNotSupportedException("Not supported.") };
        using var client = CreateClient(inner, accessor, noAnswerTimeout: TimeSpan.FromMilliseconds(50));

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

    [Test]
    public void SendAsync_NoAnswerWithoutAccessibilityPermission_FailsWithHowToGrantIt()
    {
        // Arrange
        var inner = new DelayedHandler(TimeSpan.FromMinutes(1));
        var accessor = new FakeEditorDialogAccessor { ReadException = new AccessibilityPermissionException() };
        using var client = CreateClient(inner, accessor, noAnswerTimeout: TimeSpan.FromMilliseconds(100));

        // Act
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/")));

        // Assert
        Assert.That(exception!.Message, Does.StartWith("The Unity Editor has not responded for"));
        Assert.That(exception.Message, Does.Contain("Accessibility permission is not granted"));
        Assert.That(exception.Message, Does.Contain(AccessibilityPermissionException.SettingsUrl));
        Assert.That(inner.WasCancelled, Is.True);
    }

    [Test]
    public async ValueTask SendAsync_AttemptsKeepEndingWithoutAccessibilityPermission_ReturnsResponse()
    {
        // Arrange
        var inner = new NotYetAnsweringHandler(TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(20));
        var accessor = new FakeEditorDialogAccessor { ReadException = new AccessibilityPermissionException() };
        using var client = CreateClient(inner, accessor, noAnswerTimeout: TimeSpan.FromMilliseconds(100));

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(accessor.ReadCount, Is.GreaterThan(0));
    }

    private static HttpClient CreateClient(HttpMessageHandler inner, IEditorDialogAccessor accessor,
        TimeSpan? firstCheckDelay = null, TimeSpan? noAnswerTimeout = null)
    {
        var handler = new EditorDialogWatchHandler(NullLogger<EditorDialogWatchHandler>.Instance,
            new FakeProcessProvider(), accessor)
        {
            FirstCheckDelay = firstCheckDelay ?? TimeSpan.FromMilliseconds(50),
            CheckInterval = TimeSpan.FromMilliseconds(20),
            NoAnswerTimeout = noAnswerTimeout ?? TimeSpan.FromMinutes(1),
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

    // Ends an attempt of the request at every interval, as HttpRequestHandler does while the Editor answers "not yet"
    // (e.g. GET /tests/result while tests run), and answers 200 after the delay.
    private sealed class NotYetAnsweringHandler(TimeSpan delay, TimeSpan interval) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Assert.That(request.Options.TryGetValue(RequestAttemptClock.Key, out var attemptClock), Is.True);
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < delay)
            {
                await Task.Delay(interval, cancellationToken);
                attemptClock!.MarkAttemptEnded();
            }

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

        public Exception? ReadException { get; init; }
        public int ReadCount => Volatile.Read(ref _readCount);

        public IReadOnlyList<EditorDialog> GetDialogs(int processId)
        {
            Interlocked.Increment(ref _readCount);
            if (ReadException != null)
            {
                throw ReadException;
            }

            return dialogs;
        }

        public EditorDialog? ClickButton(int processId, string buttonLabel)
        {
            throw new NotSupportedException();
        }
    }
}
