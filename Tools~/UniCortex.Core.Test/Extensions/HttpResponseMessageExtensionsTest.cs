using System.Net;
using System.Net.Http;
using System.Text;
using NUnit.Framework;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.Extensions;

[TestFixture]
public class HttpResponseMessageExtensionsTest
{
    [Test]
    public void EnsureSuccessWithErrorBodyAsync_ThrowsWithStatusCode()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.RequestTimeout)
        {
            Content = new StringContent(
                "{\"error\":\"Request was cancelled.\"}",
                Encoding.UTF8,
                "application/json")
        };

        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await response.EnsureSuccessWithErrorBodyAsync(CancellationToken.None));

        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.RequestTimeout));
        Assert.That(ex.Message, Is.EqualTo(ErrorMessages.RequestWasCancelled));
    }
}
