using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.EditorTools;

[TestFixture]
public class DomainReloadTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test, CancelAfter(120_000)]
    public async Task ReloadDomain_ReturnsSuccess()
    {
        // Arrange
        var editorTools = _fixture.EditorTools;

        // Act
        var result = await editorTools.ReloadDomain(CancellationToken.None);

        // Assert
        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("completed"));
    }

    [Test, CancelAfter(120_000), Order(2)]
    public async Task PingEditor_SucceedsAfterDomainReload()
    {
        // Arrange
        var editorTools = _fixture.EditorTools;

        // Act
        var result = await editorTools.PingEditor(CancellationToken.None);

        // Assert
        Assert.That(result.IsError, Is.Not.True);
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("pong"));
    }
}
