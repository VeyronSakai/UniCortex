using System.Net;
using System.Net.Http;
using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class TestUseCaseUnitTest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };

    [Test]
    public async ValueTask RunAsync_StartsRunThenReturnsStoredResult()
    {
        // Arrange
        var client = new FakeUnityEditorClient();
        var useCase = new TestUseCase(client);

        // Act
        var json = await useCase.RunAsync(cancellationToken: CancellationToken.None);

        // Assert
        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, s_jsonOptions)!;
        Assert.That(response.passed, Is.EqualTo(1));
        Assert.That(client.PostCallCount, Is.EqualTo(1));
        Assert.That(client.GetCallCount, Is.EqualTo(1));
        Assert.That(client.LastResendAfterDisconnect, Is.False);
    }

    [Test]
    public async ValueTask RunAsync_PollsStoredResult_WhenServerCancelsRunRequest()
    {
        // Arrange
        var client = new FakeUnityEditorClient
        {
            PostException = new HttpRequestException(ErrorMessages.ServerStopped, null,
                HttpStatusCode.ServiceUnavailable)
        };
        var useCase = new TestUseCase(client);

        // Act
        var json = await useCase.RunAsync(cancellationToken: CancellationToken.None);

        // Assert
        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, s_jsonOptions)!;
        Assert.That(response.passed, Is.EqualTo(1));
        Assert.That(client.GetCallCount, Is.EqualTo(1));
        Assert.That(client.LastResendAfterDisconnect, Is.False);
    }

    [Test]
    public async ValueTask RunAsync_PollsStoredResult_WhenConnectionIsDropped()
    {
        // Arrange
        var client = new FakeUnityEditorClient { PostException = new HttpRequestException() };
        var useCase = new TestUseCase(client);

        // Act
        var json = await useCase.RunAsync(cancellationToken: CancellationToken.None);

        // Assert
        var response = JsonSerializer.Deserialize<RunTestsResponse>(json, s_jsonOptions)!;
        Assert.That(response.passed, Is.EqualTo(1));
        Assert.That(client.GetCallCount, Is.EqualTo(1));
    }

    [Test]
    public void RunAsync_RethrowsWithoutPolling_WhenServerRejectsRunRequest()
    {
        // Arrange
        var client = new FakeUnityEditorClient
        {
            PostException = new HttpRequestException("Cannot run tests in Play Mode.", null,
                HttpStatusCode.BadRequest)
        };
        var useCase = new TestUseCase(client);

        // Act
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await useCase.RunAsync(cancellationToken: CancellationToken.None));

        // Assert
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(client.GetCallCount, Is.EqualTo(0));
    }

    private sealed class FakeUnityEditorClient : IUnityEditorClient
    {
        public Exception? PostException { get; init; }
        public int PostCallCount { get; private set; }
        public int GetCallCount { get; private set; }

        public ValueTask WaitForServerAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public bool? LastResendAfterDisconnect { get; private set; }

        public ValueTask<TRes> PostAsync<TReq, TRes>(string route, TReq? request = null,
            CancellationToken cancellationToken = default, bool resendAfterDisconnect = true) where TReq : class
        {
            PostCallCount++;
            LastResendAfterDisconnect = resendAfterDisconnect;
            if (PostException != null)
            {
                throw PostException;
            }

            if (typeof(TRes) == typeof(RunTestsAcceptedResponse))
            {
                object accepted = new RunTestsAcceptedResponse(true);
                return new ValueTask<TRes>((TRes)accepted);
            }

            throw new InvalidOperationException("Unexpected PostAsync call.");
        }

        public ValueTask<TRes> GetAsync<TReq, TRes>(string route, TReq? request = null,
            CancellationToken cancellationToken = default) where TReq : class
        {
            if (typeof(TRes) == typeof(RunTestsResponse))
            {
                GetCallCount++;
                object response = new RunTestsResponse(1, 0, 0,
                    new List<TestResultEntry> { new("StoredTest", "Passed", 0.1f) });
                return new ValueTask<TRes>((TRes)response);
            }

            throw new InvalidOperationException("Unexpected GetAsync call.");
        }
    }
}
