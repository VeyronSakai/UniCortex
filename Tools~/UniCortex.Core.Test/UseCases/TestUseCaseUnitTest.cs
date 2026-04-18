using System.Net.Http;
using NUnit.Framework;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class TestUseCaseUnitTest
{
    [Test]
    public void RunAsync_RethrowsCancellationError()
    {
        var client = new FakeUnityEditorClient
        {
            PostException = new HttpRequestException(ErrorMessages.RequestWasCancelled)
        };
        var useCase = new TestUseCase(client);

        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await useCase.RunAsync(cancellationToken: CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo(ErrorMessages.RequestWasCancelled));
    }

    private sealed class FakeUnityEditorClient : IUnityEditorClient
    {
        public Exception? PostException { get; init; }

        public ValueTask WaitForServerAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<TRes> PostAsync<TReq, TRes>(string route, TReq? request = null,
            CancellationToken cancellationToken = default) where TReq : class
        {
            if (PostException != null)
            {
                throw PostException;
            }

            throw new InvalidOperationException("PostAsync should have thrown before returning.");
        }

        public ValueTask<TRes> GetAsync<TReq, TRes>(string route, TReq? request = null,
            CancellationToken cancellationToken = default) where TReq : class
        {
            if (typeof(TRes) == typeof(RunTestsResponse))
            {
                object response = new RunTestsResponse(0, 0, 0, new List<TestResultEntry>());
                return new ValueTask<TRes>((TRes)response);
            }

            throw new InvalidOperationException("Unexpected GetAsync call.");
        }
    }
}
