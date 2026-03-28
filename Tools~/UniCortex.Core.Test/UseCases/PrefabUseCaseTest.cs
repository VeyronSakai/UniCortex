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
        await _fixture.SceneUseCase.SaveAsync(CancellationToken.None);
    }

    [TearDown]
    public async ValueTask TearDown()
    {
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        UnityEditorFixture.DeleteAssetFile(TestScenePath);
    }

    [Test]
    public async ValueTask Create_ReturnsSuccess_WhenGameObjectExists()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("CreatePrefabTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.PrefabUseCase.CreateAsync(
                createResponse.instanceId, "Assets/CreatePrefabTest.prefab", ct);

            Assert.That(message, Does.Contain("Prefab created at: Assets/CreatePrefabTest.prefab"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
            UnityEditorFixture.DeleteAssetFile("Assets/CreatePrefabTest.prefab");
        }
    }

    [Test]
    public async ValueTask Instantiate_ReturnsSuccess_WhenPrefabExists()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("InstantiatePrefabTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        await _fixture.PrefabUseCase.CreateAsync(
            createResponse.instanceId, "Assets/InstantiatePrefabTest.prefab", ct);

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

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("OpenPrefabTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            await _fixture.PrefabUseCase.CreateAsync(
                createResponse.instanceId, "Assets/OpenPrefabTest.prefab", ct);

            var message = await _fixture.PrefabUseCase.OpenAsync("Assets/OpenPrefabTest.prefab", ct);

            Assert.That(message, Does.Contain("Prefab opened: Assets/OpenPrefabTest.prefab"));

            // Create a child GameObject inside the prefab to make it dirty
            var childJson = await _fixture.GameObjectUseCase.CreateAsync("DirtyChild", ct);
            var childResponse =
                JsonSerializer.Deserialize<CreateGameObjectResponse>(childJson, s_jsonOptions)!;
            Assert.That(childResponse.instanceId, Is.Not.EqualTo(0));

            // Close prefab mode to return to the main stage
            await _fixture.PrefabUseCase.CloseAsync(ct);
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

        var message = await _fixture.PrefabUseCase.CloseAsync(ct);

        Assert.That(message, Does.Contain("Prefab mode closed."));
    }

    [Test]
    public async ValueTask GetHierarchy_InPrefabMode_ReturnsPrefabHierarchy()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("HierarchyPrefabTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            await _fixture.PrefabUseCase.CreateAsync(
                createResponse.instanceId, "Assets/HierarchyPrefabTest.prefab", ct);

            await _fixture.PrefabUseCase.OpenAsync("Assets/HierarchyPrefabTest.prefab", ct);

            var hierarchyJson = await _fixture.SceneUseCase.GetHierarchyAsync(ct);

            Assert.That(hierarchyJson, Does.Contain("HierarchyPrefabTest"));
            Assert.That(hierarchyJson, Does.Contain("HierarchyPrefabTest.prefab"));

            await _fixture.PrefabUseCase.CloseAsync(ct);
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

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("FindPrefabTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            await _fixture.PrefabUseCase.CreateAsync(
                createResponse.instanceId, "Assets/FindPrefabTest.prefab", ct);

            await _fixture.PrefabUseCase.OpenAsync("Assets/FindPrefabTest.prefab", ct);

            var findJson = await _fixture.GameObjectUseCase.FindAsync(null, ct);

            Assert.That(findJson, Does.Contain("FindPrefabTest"));

            await _fixture.PrefabUseCase.CloseAsync(ct);
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

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("FindQueryPrefabTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            await _fixture.PrefabUseCase.CreateAsync(
                createResponse.instanceId, "Assets/FindQueryPrefabTest.prefab", ct);

            await _fixture.PrefabUseCase.OpenAsync("Assets/FindQueryPrefabTest.prefab", ct);

            var findJson = await _fixture.GameObjectUseCase.FindAsync("t:Transform", ct);

            Assert.That(findJson, Does.Contain("FindQueryPrefabTest"));

            await _fixture.PrefabUseCase.CloseAsync(ct);
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

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("SavePrefabTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            await _fixture.PrefabUseCase.CreateAsync(
                createResponse.instanceId, "Assets/SavePrefabTest.prefab", ct);

            await _fixture.PrefabUseCase.OpenAsync("Assets/SavePrefabTest.prefab", ct);

            var message = await _fixture.PrefabUseCase.SaveAsync(ct);

            Assert.That(message, Does.Contain("Prefab saved."));

            await _fixture.PrefabUseCase.CloseAsync(ct);
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
            UnityEditorFixture.DeleteAssetFile("Assets/SavePrefabTest.prefab");
        }
    }
}
