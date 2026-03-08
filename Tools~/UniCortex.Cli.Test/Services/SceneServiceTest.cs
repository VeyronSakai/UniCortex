using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;

namespace UniCortex.Cli.Test.Services;

[TestFixture]
public class SceneServiceTest
{
    private const string TestScenePath = "Assets/Scenes/SceneToolsTestScene.unity";

    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
        await _fixture.SceneService.CreateAsync(TestScenePath, CancellationToken.None);
        await _fixture.AssetService.RefreshAsync(CancellationToken.None);
        // Save after refresh to prevent "Scene(s) Have Been Modified" dialog
        await _fixture.SceneService.SaveAsync(CancellationToken.None);
    }

    [OneTimeTearDown]
    public async ValueTask OneTimeTearDown()
    {
        // Save the scene to prevent "Scene(s) Have Been Modified" dialog on next scene open.
        // Do NOT call RefreshAsync after deleting to prevent "modified externally" dialog.
        await _fixture.SceneService.SaveAsync(CancellationToken.None);
        UnityEditorFixture.DeleteAssetFile(TestScenePath);
    }

    [Test]
    public async ValueTask GetHierarchy_ReturnsJsonWithSceneInfo()
    {
        await _fixture.SceneService.OpenAsync(TestScenePath, CancellationToken.None);

        var json = await _fixture.SceneService.GetHierarchyAsync(CancellationToken.None);

        Assert.That(json, Does.Contain("sceneName"));
        Assert.That(json, Does.Contain("gameObjects"));
    }

    [Test]
    public async ValueTask Open_ReturnsSuccess()
    {
        var message = await _fixture.SceneService.OpenAsync(TestScenePath, CancellationToken.None);

        Assert.That(message, Does.Contain("Scene opened"));
    }

    [Test]
    public async ValueTask Save_ReturnsSuccess()
    {
        await _fixture.SceneService.OpenAsync(TestScenePath, CancellationToken.None);

        var message = await _fixture.SceneService.SaveAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("saved successfully"));
    }

    [Test]
    public async ValueTask Create_ReturnsSuccess()
    {
        const string newScenePath = "Assets/Scenes/CreateSceneTest.unity";

        try
        {
            var message = await _fixture.SceneService.CreateAsync(newScenePath, CancellationToken.None);

            Assert.That(message, Does.Contain("Scene created"));
        }
        finally
        {
            // Re-open TestScenePath before cleanup to restore active scene.
            // Do NOT call RefreshAsync after deleting to prevent "modified externally" dialog.
            await _fixture.SceneService.OpenAsync(TestScenePath, CancellationToken.None);
            await _fixture.SceneService.SaveAsync(CancellationToken.None);
            UnityEditorFixture.DeleteAssetFile(newScenePath);
        }
    }
}
