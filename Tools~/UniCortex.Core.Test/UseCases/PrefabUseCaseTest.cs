using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class PrefabUseCaseTest
{
    private const string TestScenePath = "Assets/Scenes/PrefabToolsTestScene.unity";

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
        // Discard the test's changes: open_scene fails while a scene has unsaved changes.
        await _fixture.SceneUseCase.DiscardChangesAsync(CancellationToken.None);
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        UnityEditorFixture.DeleteAssetFile(TestScenePath);
    }

    [Test]
    public async ValueTask Create_ReturnsSuccess_WhenGameObjectExists()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("CreatePrefabTestObj", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.PrefabUseCase.CreateAsync(
                createResponse.instanceId, "Assets/CreatePrefabTest.prefab", cancellationToken: ct);

            Assert.That(message, Does.Contain("Prefab created at: Assets/CreatePrefabTest.prefab"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
            UnityEditorFixture.DeleteAssetFile("Assets/CreatePrefabTest.prefab");
        }
    }

    // Creates a GameObject with a child in the scene and saves it as a Prefab. Returns the scene GameObject's
    // instanceId. In the Prefab, the root is named after the asset file.
    private async ValueTask<int> CreatePrefabWithChildAsync(string rootName, string childName, string prefabPath,
        CancellationToken ct)
    {
        var rootJson = await _fixture.GameObjectUseCase.CreateAsync(rootName, cancellationToken: ct);
        var root = JsonSerializer.Deserialize<CreateGameObjectResponse>(rootJson, s_jsonOptions)!;
        await _fixture.GameObjectUseCase.CreateAsync(childName, parentInstanceId: root.instanceId,
            cancellationToken: ct);
        await _fixture.PrefabUseCase.CreateAsync(root.instanceId, prefabPath, cancellationToken: ct);
        return root.instanceId;
    }

    private async ValueTask<int> FindSingleInstanceIdAsync(string name, CancellationToken ct)
    {
        var findJson = await _fixture.GameObjectUseCase.FindAsync(name, ct);
        var found = JsonSerializer.Deserialize<FindGameObjectsResponse>(findJson, s_jsonOptions)!;
        return found.gameObjects.Single(g => g.name == name).instanceId;
    }

    [Test]
    public async ValueTask Create_WithConnect_InPrefabMode_LinksOpenPrefabToNewPrefab()
    {
        // Arrange
        var ct = CancellationToken.None;
        const string parentPath = "Assets/ConnectParent.prefab";
        const string partPath = "Assets/ConnectPart.prefab";
        var sceneRootId = await CreatePrefabWithChildAsync("ConnectParentObj", "ConnectPart", parentPath, ct);

        try
        {
            await _fixture.PrefabUseCase.OpenAsync(parentPath, ct);
            var partId = await FindSingleInstanceIdAsync("ConnectPart", ct);

            // Act
            var message = await _fixture.PrefabUseCase.CreateAsync(partId, partPath, connect: true,
                cancellationToken: ct);

            // Assert
            var guid = UnityEditorFixture.ReadAssetGuid(partPath);
            Assert.That(message, Does.Contain($"Prefab created at: {partPath} (GUID: {guid})."));
            Assert.That(message, Does.Contain($"GameObject {partId} is now an instance of it."));
            Assert.That(message, Does.Contain($"The Prefab open in Prefab Mode ({parentPath})"));
            // Once saved, the open Prefab holds an instance of the new Prefab.
            await _fixture.EditorUseCase.SaveAsync(ct);
            Assert.That(UnityEditorFixture.ReadAssetText(parentPath), Does.Contain(guid));
        }
        finally
        {
            await _fixture.PrefabUseCase.CloseAsync(discardChanges: true, cancellationToken: ct);
            await _fixture.GameObjectUseCase.DeleteAsync(sceneRootId, ct);
            UnityEditorFixture.DeleteAssetFile(parentPath);
            UnityEditorFixture.DeleteAssetFile(partPath);
        }
    }

    [Test]
    public async ValueTask Create_InPrefabMode_WithoutConnect_LeavesSourceUnlinked()
    {
        // Arrange
        var ct = CancellationToken.None;
        const string parentPath = "Assets/CopyParent.prefab";
        const string partPath = "Assets/CopyPart.prefab";
        var sceneRootId = await CreatePrefabWithChildAsync("CopyParentObj", "CopyPart", parentPath, ct);

        try
        {
            await _fixture.PrefabUseCase.OpenAsync(parentPath, ct);
            var partId = await FindSingleInstanceIdAsync("CopyPart", ct);

            // Act
            var message = await _fixture.PrefabUseCase.CreateAsync(partId, partPath, cancellationToken: ct);

            // Assert
            var guid = UnityEditorFixture.ReadAssetGuid(partPath);
            Assert.That(message, Does.Contain($"Prefab created at: {partPath} (GUID: {guid})."));
            Assert.That(message, Does.Not.Contain("The Prefab open in Prefab Mode"));
            // The source is not linked to the new Prefab, so it is not a prefab instance.
            var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.PrefabUseCase.UnpackAsync(partId, cancellationToken: ct));
            Assert.That(ex!.Message, Does.Contain("is not a prefab instance"));
        }
        finally
        {
            await _fixture.PrefabUseCase.CloseAsync(discardChanges: true, cancellationToken: ct);
            await _fixture.GameObjectUseCase.DeleteAsync(sceneRootId, ct);
            UnityEditorFixture.DeleteAssetFile(parentPath);
            UnityEditorFixture.DeleteAssetFile(partPath);
        }
    }

    [Test]
    public async ValueTask Create_Throws_WhenSavingOverPrefabOpenInPrefabMode()
    {
        // Arrange
        var ct = CancellationToken.None;
        const string parentPath = "Assets/OverwriteParent.prefab";
        var sceneRootId = await CreatePrefabWithChildAsync("OverwriteParentObj", "OverwritePart", parentPath, ct);

        try
        {
            var before = UnityEditorFixture.ReadAssetText(parentPath);
            await _fixture.PrefabUseCase.OpenAsync(parentPath, ct);
            var partId = await FindSingleInstanceIdAsync("OverwritePart", ct);

            // Act
            var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.PrefabUseCase.CreateAsync(partId, parentPath, cancellationToken: ct));

            // Assert
            Assert.That(ex!.Message, Does.Contain("it is the Prefab open in Prefab Mode"));
            Assert.That(UnityEditorFixture.ReadAssetText(parentPath), Is.EqualTo(before));
        }
        finally
        {
            await _fixture.PrefabUseCase.CloseAsync(discardChanges: true, cancellationToken: ct);
            await _fixture.GameObjectUseCase.DeleteAsync(sceneRootId, ct);
            UnityEditorFixture.DeleteAssetFile(parentPath);
        }
    }

    [Test]
    public async ValueTask Create_WithConnect_Throws_ForRootOfPrefabOpenInPrefabMode()
    {
        // Arrange
        var ct = CancellationToken.None;
        const string parentPath = "Assets/RootConnectParent.prefab";
        const string newPath = "Assets/RootConnectNew.prefab";
        var sceneRootId = await CreatePrefabWithChildAsync("RootConnectParentObj", "RootConnectPart", parentPath, ct);

        try
        {
            await _fixture.PrefabUseCase.OpenAsync(parentPath, ct);
            var rootId = await FindSingleInstanceIdAsync("RootConnectParent", ct);

            // Act
            var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.PrefabUseCase.CreateAsync(rootId, newPath, connect: true, cancellationToken: ct));

            // Assert
            Assert.That(ex!.Message, Does.Contain("it is the root of the Prefab open in Prefab Mode"));
            Assert.That(UnityEditorFixture.ReadAssetText(newPath), Is.Null);
        }
        finally
        {
            await _fixture.PrefabUseCase.CloseAsync(discardChanges: true, cancellationToken: ct);
            await _fixture.GameObjectUseCase.DeleteAsync(sceneRootId, ct);
            UnityEditorFixture.DeleteAssetFile(newPath);
            UnityEditorFixture.DeleteAssetFile(parentPath);
        }
    }

    [Test]
    public async ValueTask Instantiate_ReturnsSuccess_WhenPrefabExists()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("InstantiatePrefabTestObj", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        await _fixture.PrefabUseCase.CreateAsync(
            createResponse.instanceId, "Assets/InstantiatePrefabTest.prefab", cancellationToken: ct);

        var instantiatedId = 0;
        try
        {
            var json = await _fixture.PrefabUseCase.InstantiateAsync(
                "Assets/InstantiatePrefabTest.prefab", ct);

            Assert.That(json, Does.Contain("InstantiatePrefabTest"));

            var response = JsonSerializer.Deserialize<InstantiatePrefabResponse>(json, s_jsonOptions);
            Assert.That(response, Is.Not.Null);
            Assert.That(response!.instanceId, Is.Not.EqualTo(0));
            instantiatedId = response.instanceId;
        }
        finally
        {
            if (instantiatedId != 0)
            {
                await _fixture.GameObjectUseCase.DeleteAsync(instantiatedId, ct);
            }

            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
            UnityEditorFixture.DeleteAssetFile("Assets/InstantiatePrefabTest.prefab");
        }
    }

    [Test]
    public async ValueTask Open_ReturnsSuccess_WhenPrefabExists()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("OpenPrefabTestObj", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            await _fixture.PrefabUseCase.CreateAsync(
                createResponse.instanceId, "Assets/OpenPrefabTest.prefab", cancellationToken: ct);

            var message = await _fixture.PrefabUseCase.OpenAsync("Assets/OpenPrefabTest.prefab", ct);

            Assert.That(message, Does.Contain("Prefab opened: Assets/OpenPrefabTest.prefab"));

            // Create a child GameObject inside the prefab to make it dirty
            var childJson = await _fixture.GameObjectUseCase.CreateAsync("DirtyChild", cancellationToken: ct);
            var childResponse =
                JsonSerializer.Deserialize<CreateGameObjectResponse>(childJson, s_jsonOptions)!;
            Assert.That(childResponse.instanceId, Is.Not.EqualTo(0));

            // Close prefab mode to return to the main stage
            await _fixture.PrefabUseCase.CloseAsync(cancellationToken: ct);
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
            UnityEditorFixture.DeleteAssetFile("Assets/OpenPrefabTest.prefab");
        }
    }

    [Test]
    public async ValueTask Close_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var message = await _fixture.PrefabUseCase.CloseAsync(cancellationToken: ct);

        Assert.That(message, Does.Contain("Prefab mode closed."));
    }

    [Test]
    public async ValueTask Unpack_ReturnsSuccess_AndUnlinksInstance()
    {
        // Arrange
        var ct = CancellationToken.None;
        const string prefabPath = "Assets/UnpackPrefabTest.prefab";
        var createJson = await _fixture.GameObjectUseCase.CreateAsync("UnpackPrefabTestObj", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;
        await _fixture.PrefabUseCase.CreateAsync(createResponse.instanceId, prefabPath, cancellationToken: ct);
        var instantiateJson = await _fixture.PrefabUseCase.InstantiateAsync(prefabPath, ct);
        var instance = JsonSerializer.Deserialize<InstantiatePrefabResponse>(instantiateJson, s_jsonOptions)!;

        try
        {
            // Act
            var message = await _fixture.PrefabUseCase.UnpackAsync(instance.instanceId, cancellationToken: ct);

            // Assert
            Assert.That(message, Does.Contain($"Prefab instance unpacked (OutermostRoot): {instance.instanceId}"));
            // The GameObject is no longer a prefab instance, so it cannot be unpacked again.
            var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.PrefabUseCase.UnpackAsync(instance.instanceId, cancellationToken: ct));
            Assert.That(ex!.Message, Does.Contain("is not a prefab instance"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(instance.instanceId, ct);
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
            UnityEditorFixture.DeleteAssetFile(prefabPath);
        }
    }

    [Test]
    public async ValueTask Unpack_InPrefabMode_UnpacksNestedInstance()
    {
        // Arrange
        var ct = CancellationToken.None;
        const string innerPath = "Assets/UnpackInner.prefab";
        const string hostPath = "Assets/UnpackHost.prefab";
        var innerSourceJson = await _fixture.GameObjectUseCase.CreateAsync("UnpackInnerObj", cancellationToken: ct);
        var innerSource = JsonSerializer.Deserialize<CreateGameObjectResponse>(innerSourceJson, s_jsonOptions)!;
        var hostJson = await _fixture.GameObjectUseCase.CreateAsync("UnpackHostObj", cancellationToken: ct);
        var host = JsonSerializer.Deserialize<CreateGameObjectResponse>(hostJson, s_jsonOptions)!;

        try
        {
            await _fixture.PrefabUseCase.CreateAsync(innerSource.instanceId, innerPath, cancellationToken: ct);
            var innerJson = await _fixture.PrefabUseCase.InstantiateAsync(innerPath, ct);
            var inner = JsonSerializer.Deserialize<InstantiatePrefabResponse>(innerJson, s_jsonOptions)!;
            await _fixture.GameObjectUseCase.ModifyAsync(inner.instanceId, parentInstanceId: host.instanceId,
                cancellationToken: ct);
            await _fixture.PrefabUseCase.CreateAsync(host.instanceId, hostPath, cancellationToken: ct);
            await _fixture.PrefabUseCase.OpenAsync(hostPath, ct);
            var findJson = await _fixture.GameObjectUseCase.FindAsync("UnpackInner", ct);
            var found = JsonSerializer.Deserialize<FindGameObjectsResponse>(findJson, s_jsonOptions)!;
            var nestedId = found.gameObjects.Single(g => g.name == "UnpackInner").instanceId;

            // Act
            var message = await _fixture.PrefabUseCase.UnpackAsync(nestedId, cancellationToken: ct);

            // Assert
            Assert.That(message, Does.Contain("Prefab instance unpacked"));
            var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.PrefabUseCase.UnpackAsync(nestedId, cancellationToken: ct));
            Assert.That(ex!.Message, Does.Contain("is not a prefab instance"));
        }
        finally
        {
            await _fixture.PrefabUseCase.CloseAsync(discardChanges: true, cancellationToken: ct);
            await _fixture.GameObjectUseCase.DeleteAsync(host.instanceId, ct);
            await _fixture.GameObjectUseCase.DeleteAsync(innerSource.instanceId, ct);
            UnityEditorFixture.DeleteAssetFile(hostPath);
            UnityEditorFixture.DeleteAssetFile(innerPath);
        }
    }

    [Test]
    public async ValueTask Unpack_Throws_WhenModeIsUnknown()
    {
        // Arrange
        var ct = CancellationToken.None;
        var createJson = await _fixture.GameObjectUseCase.CreateAsync("UnpackModeTestObj", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            // Act
            var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.PrefabUseCase.UnpackAsync(createResponse.instanceId, "Partially", ct));

            // Assert
            Assert.That(ex!.Message, Does.Contain("Unknown unpack mode: 'Partially'"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask GetHierarchy_InPrefabMode_ReturnsPrefabHierarchy()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("HierarchyPrefabTestObj", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            await _fixture.PrefabUseCase.CreateAsync(
                createResponse.instanceId, "Assets/HierarchyPrefabTest.prefab", cancellationToken: ct);

            await _fixture.PrefabUseCase.OpenAsync("Assets/HierarchyPrefabTest.prefab", ct);

            var hierarchyJson = await _fixture.SceneUseCase.GetHierarchyAsync(ct);

            Assert.That(hierarchyJson, Does.Contain("HierarchyPrefabTest"));
            Assert.That(hierarchyJson, Does.Contain("HierarchyPrefabTest.prefab"));

            await _fixture.PrefabUseCase.CloseAsync(cancellationToken: ct);
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
            UnityEditorFixture.DeleteAssetFile("Assets/HierarchyPrefabTest.prefab");
        }
    }

    [Test]
    public async ValueTask FindGameObjects_InPrefabMode_ReturnsPrefabGameObjects()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("FindPrefabTestObj", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            await _fixture.PrefabUseCase.CreateAsync(
                createResponse.instanceId, "Assets/FindPrefabTest.prefab", cancellationToken: ct);

            await _fixture.PrefabUseCase.OpenAsync("Assets/FindPrefabTest.prefab", ct);

            var findJson = await _fixture.GameObjectUseCase.FindAsync("FindPrefabTest", ct);

            Assert.That(findJson, Does.Contain("FindPrefabTest"));

            await _fixture.PrefabUseCase.CloseAsync(cancellationToken: ct);
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
            UnityEditorFixture.DeleteAssetFile("Assets/FindPrefabTest.prefab");
        }
    }

    [Test]
    public async ValueTask FindGameObjects_WithQuery_InPrefabMode_ReturnsPrefabGameObjects()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("FindQueryPrefabTestObj", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            await _fixture.PrefabUseCase.CreateAsync(
                createResponse.instanceId, "Assets/FindQueryPrefabTest.prefab", cancellationToken: ct);

            await _fixture.PrefabUseCase.OpenAsync("Assets/FindQueryPrefabTest.prefab", ct);

            var findJson = await _fixture.GameObjectUseCase.FindAsync("t:Transform", ct);

            // Only the Prefab contents are searched, so the source object in the scene is excluded.
            var response = JsonSerializer.Deserialize<FindGameObjectsResponse>(findJson, s_jsonOptions)!;
            Assert.That(response.gameObjects.Select(g => g.name), Is.EqualTo(new[] { "FindQueryPrefabTest" }));
            Assert.That(response.gameObjects.Select(g => g.instanceId), Does.Not.Contain(createResponse.instanceId));

            await _fixture.PrefabUseCase.CloseAsync(cancellationToken: ct);
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
            UnityEditorFixture.DeleteAssetFile("Assets/FindQueryPrefabTest.prefab");
        }
    }

    [Test]
    public async ValueTask Save_ReturnsSuccess_WhenPrefabIsOpen()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("SavePrefabTestObj", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            await _fixture.PrefabUseCase.CreateAsync(
                createResponse.instanceId, "Assets/SavePrefabTest.prefab", cancellationToken: ct);

            await _fixture.PrefabUseCase.OpenAsync("Assets/SavePrefabTest.prefab", ct);

            var message = await _fixture.EditorUseCase.SaveAsync(ct);

            Assert.That(message, Does.Contain("Saved successfully."));

            await _fixture.PrefabUseCase.CloseAsync(cancellationToken: ct);
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
            UnityEditorFixture.DeleteAssetFile("Assets/SavePrefabTest.prefab");
        }
    }
}
