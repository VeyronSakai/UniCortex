using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test;

[TestFixture]
public class EditorToolsPingTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async Task PingEditor_ReturnsSuccessWithPong()
    {
        var result = await _fixture.EditorTools.PingEditor(CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));

        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("pong"));
    }
}
