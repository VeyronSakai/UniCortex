using System.Net;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UniCortex.Core.Infrastructures;

namespace UniCortex.Core.Test.Infrastructures;

[TestFixture]
public class HttpRequestHandlerTest
{
    private static HttpRequestHandler CreateHandler(HttpMessageHandler inner)
    {
        var handler = new HttpRequestHandler(new LoggerFactory().CreateLogger<HttpRequestHandler>())
        {
            InnerHandler = inner,
        };
        return handler;
    }

    private static async Task<HttpResponseMessage> SendAsync(HttpRequestHandler handler,
        CancellationToken cancellationToken = default)
    {
        using var invoker = new HttpMessageInvoker(handler);
        return await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/test"),
            cancellationToken);
    }

    [Test]
    public async Task ReturnsResponse_WhenServerReturnsSuccess()
    {
        var inner = new FakeHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("ok"),
        });
        using var handler = CreateHandler(inner);

        using var response = await SendAsync(handler);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task RetriesAndRecovers_WhenServerReturns500ThenSuccess()
    {
        var inner = new FakeHandler(
            new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("error"),
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok"),
            });
        using var handler = CreateHandler(inner);

        using var response = await SendAsync(handler);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public async Task ReturnsServerError_AfterMaxRetries()
    {
        var responses = Enumerable.Range(0, 6)
            .Select(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("error"),
            })
            .ToArray();
        var inner = new FakeHandler(responses);
        using var handler = CreateHandler(inner);

        using var response = await SendAsync(handler);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        // 1 initial + 5 retries = 6 calls
        Assert.That(inner.CallCount, Is.EqualTo(6));
    }

    private sealed class FakeHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private int _callIndex;

        public int CallCount => _callIndex;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var index = Math.Min(_callIndex, responses.Length - 1);
            _callIndex++;
            return Task.FromResult(responses[index]);
        }
    }
}
