using System.Net.Http;
using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class GameObjectUseCaseTest
{
    private const string TestScenePath = "Assets/Scenes/GameObjectToolsTestScene.unity";

    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [SetUp]
    public async ValueTask SetUp()
    {
        await _fixture.SceneUseCase.CreateAsync(TestScenePath, CancellationToken.None);
        await _fixture.AssetUseCase.RefreshAsync(CancellationToken.None);
        await _fixture.EditorUseCase.SaveAsync(CancellationToken.None);
    }

    [TearDown]
    public async ValueTask TearDown()
    {
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        UnityEditorFixture.DeleteAssetFile(TestScenePath);
    }

    [Test]
    public void Find_WithNullQuery_ThrowsException()
    {
        Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.GameObjectUseCase.FindAsync(null!, CancellationToken.None));
    }

    [Test]
    public void Find_WithEmptyQuery_ThrowsException()
    {
        Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.GameObjectUseCase.FindAsync("", CancellationToken.None));
    }

    [Test]
    public async ValueTask Find_WithQuery_ReturnsFilteredResults()
    {
        var result = await _fixture.GameObjectUseCase.FindAsync("t:Camera", CancellationToken.None);

        Assert.That(result, Does.Contain("gameObjects"));
    }

    [Test]
    public async ValueTask Find_ReturnsSceneNameOfEachResult()
    {
        // Arrange
        var ct = CancellationToken.None;
        var created = await CreateAsync("FindSceneNameTarget", ct);

        // Act
        var json = await _fixture.GameObjectUseCase.FindAsync("FindSceneNameTarget", ct);

        // Assert
        var response = JsonSerializer.Deserialize<FindGameObjectsResponse>(json, s_jsonOptions)!;
        Assert.That(response.gameObjects, Has.Count.EqualTo(1));
        Assert.That(response.gameObjects[0].sceneName, Is.EqualTo("GameObjectToolsTestScene"));
    }

    [Test]
    public async ValueTask CreateAndDelete_WorksEndToEnd()
    {
        var createJson = await _fixture.GameObjectUseCase.CreateAsync("TestObj", cancellationToken: CancellationToken.None);

        Assert.That(createJson, Does.Contain("TestObj"));

        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions);
        Assert.That(createResponse, Is.Not.Null);

        var deleteMessage =
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse!.instanceId, CancellationToken.None);

        Assert.That(deleteMessage, Does.Contain("deleted"));
    }

    [Test]
    public async ValueTask Modify_ChangeName_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("ModifyNameTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.GameObjectUseCase.ModifyAsync(createResponse.instanceId,
                name: "RenamedObj", cancellationToken: ct);

            Assert.That(message, Does.Contain("modified successfully"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Modify_ChangeActiveSelf_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("ModifyActiveTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.GameObjectUseCase.ModifyAsync(createResponse.instanceId,
                activeSelf: false, cancellationToken: ct);

            Assert.That(message, Does.Contain("modified successfully"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Modify_ChangeTag_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("ModifyTagTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.GameObjectUseCase.ModifyAsync(createResponse.instanceId,
                tag: "EditorOnly", cancellationToken: ct);

            Assert.That(message, Does.Contain("modified successfully"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Modify_ChangeLayer_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("ModifyLayerTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.GameObjectUseCase.ModifyAsync(createResponse.instanceId,
                layer: 1, cancellationToken: ct);

            Assert.That(message, Does.Contain("modified successfully"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Modify_ChangeParent_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var parentJson = await _fixture.GameObjectUseCase.CreateAsync("ParentObj", cancellationToken: ct);
        var parentResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(parentJson, s_jsonOptions)!;

        var childJson = await _fixture.GameObjectUseCase.CreateAsync("ChildObj", cancellationToken: ct);
        var childResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(childJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.GameObjectUseCase.ModifyAsync(childResponse.instanceId,
                parentInstanceId: parentResponse.instanceId, cancellationToken: ct);

            Assert.That(message, Does.Contain("modified successfully"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(childResponse.instanceId, ct);
            await _fixture.GameObjectUseCase.DeleteAsync(parentResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Modify_MultipleProperties_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("ModifyMultiTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.GameObjectUseCase.ModifyAsync(createResponse.instanceId,
                name: "MultiModified", activeSelf: false, tag: "EditorOnly", layer: 1,
                cancellationToken: ct);

            Assert.That(message, Does.Contain("modified successfully"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Duplicate_CreatesCopyWithNewInstanceId()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("DuplicateSrc", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        DuplicateGameObjectResponse? duplicateResponse = null;
        try
        {
            var duplicateJson =
                await _fixture.GameObjectUseCase.DuplicateAsync(createResponse.instanceId, null, ct);
            duplicateResponse = JsonSerializer.Deserialize<DuplicateGameObjectResponse>(duplicateJson, s_jsonOptions)!;

            Assert.That(duplicateResponse, Is.Not.Null);
            Assert.That(duplicateResponse.instanceId, Is.Not.EqualTo(createResponse.instanceId));
            // No explicit name supplied, so a Unity-style unique sibling name is assigned.
            Assert.That(duplicateResponse.name, Is.EqualTo("DuplicateSrc (1)"));
        }
        finally
        {
            if (duplicateResponse != null)
            {
                await _fixture.GameObjectUseCase.DeleteAsync(duplicateResponse.instanceId, ct);
            }

            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Duplicate_WithName_UsesProvidedName()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("DuplicateNamedSrc", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        DuplicateGameObjectResponse? duplicateResponse = null;
        try
        {
            var duplicateJson =
                await _fixture.GameObjectUseCase.DuplicateAsync(createResponse.instanceId, "CustomCopy", ct);
            duplicateResponse = JsonSerializer.Deserialize<DuplicateGameObjectResponse>(duplicateJson, s_jsonOptions)!;

            Assert.That(duplicateResponse.name, Is.EqualTo("CustomCopy"));
        }
        finally
        {
            if (duplicateResponse != null)
            {
                await _fixture.GameObjectUseCase.DeleteAsync(duplicateResponse.instanceId, ct);
            }

            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Create_WithParentAndSiblingIndex_PlacesObjectAtIndex()
    {
        // Arrange
        var ct = CancellationToken.None;
        var parent = await CreateAsync("SiblingParent", ct);
        try
        {
            await CreateAsync("ChildA", ct, parent.instanceId);
            await CreateAsync("ChildB", ct, parent.instanceId);

            // Act
            var inserted = await CreateAsync("Inserted", ct, parent.instanceId, siblingIndex: 1);

            // Assert
            var children = await GetChildNamesAsync(parent.instanceId, ct);
            Assert.That(children, Is.EqualTo(new[] { "ChildA", "Inserted", "ChildB" }));
            Assert.That(inserted.name, Is.EqualTo("Inserted"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(parent.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Create_WithUseRectTransform_UsesRectTransform()
    {
        // Arrange
        var ct = CancellationToken.None;
        var parent = await CreateAsync("RectParent", ct);
        try
        {
            // Act
            var child = await CreateAsync("RectChild", ct, parent.instanceId, useRectTransform: true);

            // Assert
            var json = await _fixture.GameObjectUseCase.FindAsync("RectChild", ct);
            var response = JsonSerializer.Deserialize<FindGameObjectsResponse>(json, s_jsonOptions)!;
            var found = response.gameObjects.Single(g => g.instanceId == child.instanceId);
            Assert.That(found.components, Does.Contain("UnityEngine.RectTransform"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(parent.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Modify_WithSiblingIndexOnly_ReordersWithinSameParent()
    {
        // Arrange
        var ct = CancellationToken.None;
        var parent = await CreateAsync("ReorderParent", ct);
        try
        {
            await CreateAsync("ChildA", ct, parent.instanceId);
            await CreateAsync("ChildB", ct, parent.instanceId);
            var childC = await CreateAsync("ChildC", ct, parent.instanceId);

            // Act
            await _fixture.GameObjectUseCase.ModifyAsync(childC.instanceId, siblingIndex: 0, cancellationToken: ct);

            // Assert
            var children = await GetChildNamesAsync(parent.instanceId, ct);
            Assert.That(children, Is.EqualTo(new[] { "ChildC", "ChildA", "ChildB" }));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(parent.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Modify_WithParentAndSiblingIndex_ReparentsAtIndex()
    {
        // Arrange
        var ct = CancellationToken.None;
        var parent = await CreateAsync("ReparentTarget", ct);
        var mover = await CreateAsync("Mover", ct);
        try
        {
            await CreateAsync("ChildA", ct, parent.instanceId);
            await CreateAsync("ChildB", ct, parent.instanceId);

            // Act
            await _fixture.GameObjectUseCase.ModifyAsync(mover.instanceId, parentInstanceId: parent.instanceId,
                siblingIndex: 1, worldPositionStays: false, cancellationToken: ct);

            // Assert
            var children = await GetChildNamesAsync(parent.instanceId, ct);
            Assert.That(children, Is.EqualTo(new[] { "ChildA", "Mover", "ChildB" }));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(parent.instanceId, ct);
        }
    }

    private async ValueTask<CreateGameObjectResponse> CreateAsync(string name, CancellationToken ct,
        int? parentInstanceId = null, int? siblingIndex = null, bool? useRectTransform = null)
    {
        var json = await _fixture.GameObjectUseCase.CreateAsync(name, parentInstanceId, siblingIndex,
            useRectTransform, cancellationToken: ct);
        return JsonSerializer.Deserialize<CreateGameObjectResponse>(json, s_jsonOptions)!;
    }

    private async ValueTask<string[]> GetChildNamesAsync(int parentInstanceId, CancellationToken ct)
    {
        var json = await _fixture.SceneUseCase.GetHierarchyAsync(ct);
        var hierarchy = JsonSerializer.Deserialize<GetHierarchyResponse>(json, s_jsonOptions)!;
        var parent = hierarchy.scenes.SelectMany(scene => scene.gameObjects)
            .First(node => node.instanceId == parentInstanceId);
        return parent.children.Select(node => node.name).ToArray();
    }
}
