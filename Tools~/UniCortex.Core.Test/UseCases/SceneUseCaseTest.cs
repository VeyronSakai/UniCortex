using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class SceneUseCaseTest
{
    private const string TestScenePath = "Assets/Scenes/SceneToolsTestScene.unity";

    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
        await _fixture.SceneUseCase.CreateAsync(TestScenePath, CancellationToken.None);
        await _fixture.AssetUseCase.RefreshAsync(CancellationToken.None);
        // Save after refresh to prevent "Scene(s) Have Been Modified" dialog
        await _fixture.EditorUseCase.SaveAsync(CancellationToken.None);
    }

    [OneTimeTearDown]
    public async ValueTask OneTimeTearDown()
    {
        // Open SampleScene first so the test scene is unloaded from Unity.
        // OpenScene calls SaveIfDirty(), which would recreate the file if we deleted it first.
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
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
    public async ValueTask GetHierarchy_ReturnsActiveSceneEntry()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestScenePath, CancellationToken.None);

        // Act
        var json = await _fixture.SceneUseCase.GetHierarchyAsync(CancellationToken.None);

        // Assert
        var response = JsonSerializer.Deserialize<GetHierarchyResponse>(json, s_jsonOptions)!;
        Assert.That(response.scenes, Has.Count.EqualTo(1));
        Assert.That(response.scenes[0].sceneName, Is.EqualTo("SceneToolsTestScene"));
        Assert.That(response.scenes[0].scenePath, Is.EqualTo(TestScenePath));
        Assert.That(response.scenes[0].isActive, Is.True);
    }

    [Test]
    public async ValueTask Open_ReturnsSuccess()
    {
        var message = await _fixture.SceneUseCase.OpenAsync(TestScenePath, CancellationToken.None);

        Assert.That(message, Does.Contain("Scene opened"));
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
            await _fixture.EditorUseCase.SaveAsync(CancellationToken.None);
            UnityEditorFixture.DeleteAssetFile(newScenePath);
        }
    }
}
