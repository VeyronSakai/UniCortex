using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using UniCortex.Core.Domains;
using UniCortex.Core.Extensions;

namespace UniCortex.Core.Test.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTest
{
    [Test]
    public void AddUniCortexCore_UniCortexHttpClient_HasNoTimeLimit()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUniCortexCore();
        using var provider = services.BuildServiceProvider();

        // Act
        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientNames.UniCortex);

        // Assert
        Assert.That(client.Timeout, Is.EqualTo(Timeout.InfiniteTimeSpan));
    }
}
