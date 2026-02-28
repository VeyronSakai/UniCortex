using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.SceneTools;

[TestFixture]
public class SceneToolsTest
{
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async Task GetSceneHierarchy_ReturnsJsonWithSceneInfo()
    {
        var sceneTools = _fixture.SceneTools;

        var openResult = await sceneTools.OpenScene("Assets/Scenes/SampleScene.unity", CancellationToken.None);
        Assert.That(openResult.IsError, Is.Not.True);

        var result = await sceneTools.GetSceneHierarchy(CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("sceneName"));
        Assert.That(text, Does.Contain("gameObjects"));
    }

    [Test]
    public async Task OpenScene_ReturnsSuccess()
    {
        var sceneTools = _fixture.SceneTools;

        var result = await sceneTools.OpenScene("Assets/Scenes/SampleScene.unity", CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("Scene opened"));
    }

    [Test]
    public async Task SaveScene_ReturnsSuccess()
    {
        var sceneTools = _fixture.SceneTools;

        var openResult = await sceneTools.OpenScene("Assets/Scenes/SampleScene.unity", CancellationToken.None);
        Assert.That(openResult.IsError, Is.Not.True);

        var result = await sceneTools.SaveScene(CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("saved successfully"));
    }
}
