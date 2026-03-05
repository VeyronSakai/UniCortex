using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.EditorTools;

[TestFixture]
public class PingTest
{
    private EditorToolsFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await EditorToolsFixture.CreateAsync();
    }

    [Test]
    public async ValueTask PingEditor_ReturnsSuccessWithPong()
    {
        // Arrange
        var editorTools = _fixture.EditorTools;

        // Act
        var result = await editorTools.PingEditor(CancellationToken.None);

        // Assert
        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("pong"));
    }
}
