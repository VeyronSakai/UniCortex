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

    [Test]
    public void EnsureSuccessWithErrorBodyAsync_ServiceUnavailable_ThrowsWithErrorMessage()
    {
        // Arrange
        const string message = "The Unity Editor main thread has not responded for 30 seconds.";
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent(
                $"{{\"error\":\"{message}\"}}",
                Encoding.UTF8,
                "application/json")
        };

        // Act
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await response.EnsureSuccessWithErrorBodyAsync(CancellationToken.None));

        // Assert
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(ex.Message, Is.EqualTo(message));
    }
}
