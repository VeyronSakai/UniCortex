using ModelContextProtocol.Protocol;
using NUnit.Framework;
using UniCortex.Mcp.Domains.Interfaces;
using UniCortex.Mcp.Tools;

namespace UniCortex.Mcp.Test.EditorTools;

[TestFixture]
public class ErrorHandlingTest
{
    [Test]
    public async Task PingEditor_ReturnsMessageOnly_WhenUrlProviderThrows()
    {
        var tools = new UniCortex.Mcp.Tools.EditorTools(new DummyHttpClientFactory(), new ThrowingUrlProvider());

        var result = await tools.PingEditor(CancellationToken.None);

        Assert.That(result.IsError, Is.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Is.EqualTo("bad url"));
        Assert.That(text, Does.Not.Contain("InvalidOperationException"));
    }

    private sealed class ThrowingUrlProvider : IUnityServerUrlProvider
    {
        public string GetUrl() => throw new InvalidOperationException("bad url");
    }

    private sealed class DummyHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
