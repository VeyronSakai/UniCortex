using ModelContextProtocol.Protocol;
using NUnit.Framework;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Test.EditorTools;

[TestFixture]
public class ErrorHandlingTest
{
    [Test]
    public async Task PingEditor_ReturnsMessageOnly_WhenUrlProviderThrows()
    {
        var editorService = new EditorUseCase(new DummyHttpClientFactory(), new ThrowingUrlProvider());
        var tools = new UniCortex.Mcp.Tools.EditorTools(editorService);

        var result = await tools.PingEditorAsync(CancellationToken.None);

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
