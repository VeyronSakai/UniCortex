using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class SceneUseCaseTest
{
    private const string TestScenePath = "Assets/Scenes/SceneToolsTestScene.unity";

    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
        await _fixture.SceneUseCase.CreateAsync(TestScenePath, CancellationToken.None);
        await _fixture.AssetUseCase.RefreshAsync(CancellationToken.None);
        // Save after refresh to prevent "Scene(s) Have Been Modified" dialog
        await _fixture.SceneUseCase.SaveAsync(CancellationToken.None);
    }

    [OneTimeTearDown]
    public async ValueTask OneTimeTearDown()
    {
        // Open SampleScene first so the test scene is unloaded from Unity.
        // OpenScene calls SaveIfDirty(), which would recreate the file if we deleted it first.
        await _fixture.SceneUseCase.OpenAsync("Assets/Scenes/SampleScene.unity", CancellationToken.None);
        UnityEditorFixture.DeleteAssetFile(TestScenePath);
    }

    [Test]
    public async ValueTask GetHierarchy_ReturnsJsonWithSceneInfo()
    {
        await _fixture.SceneUseCase.OpenAsync(TestScenePath, CancellationToken.None);

        var json = await _fixture.SceneUseCase.GetHierarchyAsync(CancellationToken.None);

        Assert.That(json, Does.Contain("sceneName"));
        Assert.That(json, Does.Contain("gameObjects"));
    }

    [Test]
    public async ValueTask Open_ReturnsSuccess()
    {
        var message = await _fixture.SceneUseCase.OpenAsync(TestScenePath, CancellationToken.None);

        Assert.That(message, Does.Contain("Scene opened"));
    }

    [Test]
    public async ValueTask Save_ReturnsSuccess()
    {
        await _fixture.SceneUseCase.OpenAsync(TestScenePath, CancellationToken.None);

        var message = await _fixture.SceneUseCase.SaveAsync(CancellationToken.None);

        Assert.That(message, Does.Contain("saved successfully"));
    }

    [Test]
    public async ValueTask Create_ReturnsSuccess()
    {
        const string newScenePath = "Assets/Scenes/CreateSceneTest.unity";

        try
        {
            var message = await _fixture.SceneUseCase.CreateAsync(newScenePath, CancellationToken.None);

            Assert.That(message, Does.Contain("Scene created"));
        }
        finally
        {
            // Re-open TestScenePath before cleanup to restore active scene.
            // Do NOT call RefreshAsync after deleting to prevent "modified externally" dialog.
            await _fixture.SceneUseCase.OpenAsync(TestScenePath, CancellationToken.None);
            await _fixture.SceneUseCase.SaveAsync(CancellationToken.None);
            UnityEditorFixture.DeleteAssetFile(newScenePath);
        }
    }
}
