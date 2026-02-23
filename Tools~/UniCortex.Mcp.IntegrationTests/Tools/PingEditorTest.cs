using ModelContextProtocol.Protocol;
using UniCortex.Mcp.IntegrationTests;

namespace UniCortex.Mcp.IntegrationTests.Tools;

[TestFixture]
[Category("Integration")]
public class PingEditorTest
{
    [Test]
    public async Task PingEditor_ReturnsPong()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        var result = await UnityEditorFixture.EditorTools.PingEditor(cts.Token);

        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(result.IsError, Is.Not.True, $"EditorTools returned error: {text}");
        Assert.That(text, Does.Contain("pong"));
    }
}
