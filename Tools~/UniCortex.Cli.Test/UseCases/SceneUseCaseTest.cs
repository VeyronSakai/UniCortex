using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;

namespace UniCortex.Cli.Test.UseCases;

[TestFixture]
public class SceneUseCaseTest
{
    private const string TestScenePath = "Assets/Scenes/SceneToolsTestScene.unity";

    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
        await _fixture.EnsureNotInPlayModeAsync(CancellationToken.None);
        await _fixture.SceneUseCase.CreateAsync(TestScenePath, CancellationToken.None);
        await _fixture.AssetUseCase.RefreshAsync(CancellationToken.None);
        // Save after refresh to prevent "Scene(s) Have Been Modified" dialog
        await _fixture.SceneUseCase.SaveAsync(CancellationToken.None);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
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
