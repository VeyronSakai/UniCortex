using System.Net.Http;
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
        // Discard the test's changes: open_scene fails while a scene has unsaved changes.
        await _fixture.SceneUseCase.DiscardChangesAsync(CancellationToken.None);
        // Open SampleScene first so the test scene is unloaded from Unity before its file is deleted.
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
    public async ValueTask Open_Throws_WhenSceneHasUnsavedChanges()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestScenePath, CancellationToken.None);
        await _fixture.GameObjectUseCase.CreateAsync("UnsavedObject", cancellationToken: CancellationToken.None);

        try
        {
            // Act
            var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None));

            // Assert
            Assert.That(ex!.Message, Does.Contain("Cannot open a scene"));
            Assert.That(ex.Message, Does.Contain(TestScenePath));
            Assert.That(ex.Message, Does.Contain("discard_scene_changes"));
        }
        finally
        {
            await _fixture.SceneUseCase.DiscardChangesAsync(CancellationToken.None);
        }
    }

    [Test]
    public async ValueTask DiscardChanges_ReloadsSceneFromDisk()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestScenePath, CancellationToken.None);
        await _fixture.GameObjectUseCase.CreateAsync("DiscardedObject", cancellationToken: CancellationToken.None);

        // Act
        var message = await _fixture.SceneUseCase.DiscardChangesAsync(CancellationToken.None);

        // Assert
        Assert.That(message, Does.Contain("Scene changes discarded"));
        var json = await _fixture.SceneUseCase.GetHierarchyAsync(CancellationToken.None);
        var response = JsonSerializer.Deserialize<GetHierarchyResponse>(json, s_jsonOptions)!;
        Assert.That(response.scenes, Has.Count.EqualTo(1));
        Assert.That(response.scenes[0].scenePath, Is.EqualTo(TestScenePath));
        Assert.That(response.scenes[0].isActive, Is.True);
        Assert.That(response.scenes[0].gameObjects.Select(g => g.name), Does.Not.Contain("DiscardedObject"));

        // The scene has no unsaved changes left, so another scene can be opened.
        Assert.DoesNotThrowAsync(async () =>
            await _fixture.SceneUseCase.OpenAsync(TestScenePath, CancellationToken.None));
    }

    [Test]
    public async ValueTask SaveAs_SavesActiveSceneToNewPath()
    {
        // Arrange
        const string savedAsScenePath = "Assets/Scenes/SaveSceneAsTest.unity";
        await _fixture.SceneUseCase.OpenAsync(TestScenePath, CancellationToken.None);
        await _fixture.GameObjectUseCase.CreateAsync("SavedAsObject", cancellationToken: CancellationToken.None);

        try
        {
            // Act
            var message = await _fixture.SceneUseCase.SaveAsAsync(savedAsScenePath, CancellationToken.None);

            // Assert
            Assert.That(message, Does.Contain($"Active scene saved as: {savedAsScenePath}"));
            var json = await _fixture.SceneUseCase.GetHierarchyAsync(CancellationToken.None);
            var response = JsonSerializer.Deserialize<GetHierarchyResponse>(json, s_jsonOptions)!;
            Assert.That(response.scenes, Has.Count.EqualTo(1));
            Assert.That(response.scenes[0].scenePath, Is.EqualTo(savedAsScenePath));
            Assert.That(response.scenes[0].gameObjects.Select(g => g.name), Does.Contain("SavedAsObject"));

            // The saved scene has no unsaved changes left, so another scene can be opened. The scene at the
            // previous path keeps its last saved state.
            await _fixture.SceneUseCase.OpenAsync(TestScenePath, CancellationToken.None);
            json = await _fixture.SceneUseCase.GetHierarchyAsync(CancellationToken.None);
            response = JsonSerializer.Deserialize<GetHierarchyResponse>(json, s_jsonOptions)!;
            Assert.That(response.scenes[0].gameObjects.Select(g => g.name), Does.Not.Contain("SavedAsObject"));
        }
        finally
        {
            await _fixture.SceneUseCase.DiscardChangesAsync(CancellationToken.None);
            await _fixture.SceneUseCase.OpenAsync(TestScenePath, CancellationToken.None);
            UnityEditorFixture.DeleteAssetFile(savedAsScenePath);
        }
    }

    [Test]
    public async ValueTask SaveAs_Throws_WhenPathAlreadyExists()
    {
        // Arrange
        await _fixture.SceneUseCase.OpenAsync(TestScenePath, CancellationToken.None);

        // Act
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.SceneUseCase.SaveAsAsync(TestConstants.SampleScenePath, CancellationToken.None));

        // Assert
        Assert.That(ex!.Message, Does.Contain("already exists"));
        var json = await _fixture.SceneUseCase.GetHierarchyAsync(CancellationToken.None);
        var response = JsonSerializer.Deserialize<GetHierarchyResponse>(json, s_jsonOptions)!;
        Assert.That(response.scenes[0].scenePath, Is.EqualTo(TestScenePath));
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
