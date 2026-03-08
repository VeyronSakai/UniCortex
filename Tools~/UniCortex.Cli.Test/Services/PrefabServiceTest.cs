using System.Text.Json;
using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Test.Services;

[TestFixture]
public class PrefabServiceTest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask Create_ReturnsSuccess_WhenGameObjectExists()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectService.CreateAsync("CreatePrefabTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.PrefabService.CreateAsync(
                createResponse.instanceId, "Assets/CreatePrefabTest.prefab", ct);

            Assert.That(message, Does.Contain("Prefab created at: Assets/CreatePrefabTest.prefab"));
        }
        finally
        {
            await _fixture.GameObjectService.DeleteAsync(createResponse.instanceId, ct);
            UnityEditorFixture.DeleteAssetFile("Assets/CreatePrefabTest.prefab");
        }
    }

    [Test]
    public async ValueTask Instantiate_ReturnsSuccess_WhenPrefabExists()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectService.CreateAsync("InstantiatePrefabTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        await _fixture.PrefabService.CreateAsync(
            createResponse.instanceId, "Assets/InstantiatePrefabTest.prefab", ct);

        var instantiatedId = 0;
        try
        {
            var json = await _fixture.PrefabService.InstantiateAsync(
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
                await _fixture.GameObjectService.DeleteAsync(instantiatedId, ct);
            }

            await _fixture.GameObjectService.DeleteAsync(createResponse.instanceId, ct);
            UnityEditorFixture.DeleteAssetFile("Assets/InstantiatePrefabTest.prefab");
        }
    }
}
