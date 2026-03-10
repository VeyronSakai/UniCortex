using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.SceneTools;

[TestFixture]
public class SceneToolsTest
{
    private const string TestScenePath = "Assets/Scenes/SceneToolsTestScene.unity";

    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
        await _fixture.EnsureNotInPlayModeAsync(CancellationToken.None);
        await _fixture.SceneTools.CreateSceneAsync(TestScenePath, CancellationToken.None);
        await _fixture.AssetTools.RefreshAssetDatabaseAsync(CancellationToken.None);
        // Save after refresh to prevent "Scene(s) Have Been Modified" dialog
        await _fixture.SceneTools.SaveSceneAsync(CancellationToken.None);
    }

    [OneTimeTearDown]
    public async ValueTask OneTimeTearDown()
    {
        // Open SampleScene first so the test scene is unloaded from Unity.
        // OpenScene calls SaveIfDirty(), which would recreate the file if we deleted it first.
        await _fixture.SceneTools.OpenSceneAsync("Assets/Scenes/SampleScene.unity", CancellationToken.None);
        UnityEditorFixture.DeleteAssetFile(TestScenePath);
    }

    [Test]
    public async ValueTask GetSceneHierarchy_ReturnsJsonWithSceneInfo()
    {
        var sceneTools = _fixture.SceneTools;

        var openResult = await sceneTools.OpenSceneAsync(TestScenePath, CancellationToken.None);
        Assert.That(openResult.IsError, Is.Not.True);

        var result = await sceneTools.GetSceneHierarchyAsync(CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("sceneName"));
        Assert.That(text, Does.Contain("gameObjects"));
    }

    [Test]
    public async ValueTask OpenScene_ReturnsSuccess()
    {
        var sceneTools = _fixture.SceneTools;

        var result = await sceneTools.OpenSceneAsync(TestScenePath, CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("Scene opened"));
    }

    [Test]
    public async ValueTask SaveScene_ReturnsSuccess()
    {
        var sceneTools = _fixture.SceneTools;

        var openResult = await sceneTools.OpenSceneAsync(TestScenePath, CancellationToken.None);
        Assert.That(openResult.IsError, Is.Not.True);

        var result = await sceneTools.SaveSceneAsync(CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("saved successfully"));
    }

    [Test]
    public async ValueTask CreateScene_ReturnsSuccess()
    {
        var sceneTools = _fixture.SceneTools;
        const string newScenePath = "Assets/Scenes/CreateSceneTest.unity";

        try
        {
            var result = await sceneTools.CreateSceneAsync(newScenePath, CancellationToken.None);

            Assert.That(result.IsError, Is.Not.True);
            Assert.That(result.Content, Has.Count.EqualTo(1));
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("Scene created"));
        }
        finally
        {
            // Re-open TestScenePath before cleanup to restore active scene.
            // Do NOT call RefreshAssetDatabase after deleting to prevent "modified externally" dialog.
            await sceneTools.OpenSceneAsync(TestScenePath, CancellationToken.None);
            await sceneTools.SaveSceneAsync(CancellationToken.None);
            UnityEditorFixture.DeleteAssetFile(newScenePath);
        }
    }
}
