using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.GameObjectTools;

[TestFixture]
public class GameObjectToolsTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async Task FindGameObjects_ReturnsJsonWithGameObjects()
    {
        var tools = _fixture.GameObjectTools;

        var result = await tools.FindGameObjects(cancellationToken: CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("gameObjects"));
    }

    [Test]
    public async Task CreateAndDeleteGameObject_WorksEndToEnd()
    {
        var tools = _fixture.GameObjectTools;

        var createResult = await tools.CreateGameObject("TestObj", cancellationToken: CancellationToken.None);

        Assert.That(createResult.IsError, Is.Not.True);
        var text = ((TextContentBlock)createResult.Content[0]).Text;
        Assert.That(text, Does.Contain("TestObj"));
    }
}
