using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.MenuItemTools;

[TestFixture]
public class MenuItemToolsTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async Task ExecuteMenuItem_ReturnsSuccess()
    {
        var menuItemTools = _fixture.MenuItemTools;

        var result = await menuItemTools.ExecuteMenuItem("Edit/Select All", CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("executed"));
    }
}
