using NUnit.Framework;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class EditorUseCaseUnitTest
{
    [Test, CancelAfter(10_000)]
    public async ValueTask ReloadDomainAsync_WaitsUntilDomainIdChanges(CancellationToken cancellationToken)
    {
        // Arrange
        // The old domain keeps answering for a while after the reload request.
        var client = new FakeUnityEditorClient(
            new PingResponse("ok", "pong", "old", 0),
            new PingResponse("ok", "pong", "old", 0),
            new PingResponse("ok", "pong", "old", 0),
            new PingResponse("ok", "pong", "new", 0));
        var useCase = new EditorUseCase(client);

        // Act
        var message = await useCase.ReloadDomainAsync(cancellationToken);

        // Assert
        Assert.That(message, Does.Contain("completed"));
        Assert.That(client.PingCallCount, Is.EqualTo(4));
        Assert.That(client.DomainReloadCallCount, Is.EqualTo(1));
    }

    [Test, CancelAfter(10_000)]
    public void ReloadDomainAsync_Throws_WhenCompilationFails(CancellationToken cancellationToken)
    {
        // Arrange
        var client = new FakeUnityEditorClient(
            new PingResponse("ok", "pong", "old", 1),
            new PingResponse("ok", "pong", "old", 1),
            new PingResponse("ok", "pong", "old", 2));
        var useCase = new EditorUseCase(client);

        // Act
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await useCase.ReloadDomainAsync(cancellationToken));

        // Assert
        Assert.That(ex!.Message, Does.Contain("compilation failed"));
        Assert.That(client.PingCallCount, Is.EqualTo(3));
    }

    private sealed class FakeUnityEditorClient(params PingResponse[] pingResponses) : IUnityEditorClient
    {
        private readonly Queue<PingResponse> _pingResponses = new(pingResponses);

        public int PingCallCount { get; private set; }
        public int DomainReloadCallCount { get; private set; }

        public ValueTask WaitForServerAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<TRes> PostAsync<TReq, TRes>(string route, TReq? request = null,
            CancellationToken cancellationToken = default) where TReq : class
        {
            if (route == ApiRoutes.DomainReload)
            {
                DomainReloadCallCount++;
                object response = new DomainReloadResponse(true);
                return new ValueTask<TRes>((TRes)response);
            }

            throw new InvalidOperationException("Unexpected PostAsync call.");
        }

        public ValueTask<TRes> GetAsync<TReq, TRes>(string route, TReq? request = null,
            CancellationToken cancellationToken = default) where TReq : class
        {
            if (route == ApiRoutes.Ping)
            {
                PingCallCount++;
                object response = _pingResponses.Dequeue();
                return new ValueTask<TRes>((TRes)response);
            }

            throw new InvalidOperationException("Unexpected GetAsync call.");
        }
    }
}
