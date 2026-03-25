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
