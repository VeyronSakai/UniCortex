using System.Text.Json;
using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.PrefabTools;

[TestFixture]
public class PrefabToolsTest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask CreatePrefab_ReturnsSuccess_WhenGameObjectExists()
    {
        var ct = CancellationToken.None;

        var createResult =
            await _fixture.GameObjectTools.CreateGameObjectAsync("CreatePrefabTestObj", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await _fixture.PrefabTools.CreatePrefabAsync(
                createResponse.instanceId,
                "Assets/CreatePrefabTest.prefab",
                ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("Prefab created at: Assets/CreatePrefabTest.prefab"));
        }
        finally
        {
            await _fixture.GameObjectTools.DeleteGameObjectAsync(createResponse.instanceId, cancellationToken: ct);
            UnityEditorFixture.DeleteAssetFile("Assets/CreatePrefabTest.prefab");
        }
    }

    [Test]
    public async ValueTask InstantiatePrefab_ReturnsSuccess_WhenPrefabExists()
    {
        var ct = CancellationToken.None;

        var createResult =
            await _fixture.GameObjectTools.CreateGameObjectAsync("InstantiatePrefabTestObj", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        await _fixture.PrefabTools.CreatePrefabAsync(
            createResponse.instanceId,
            "Assets/InstantiatePrefabTest.prefab",
            ct);

        var instantiatedId = 0;
        try
        {
            var result = await _fixture.PrefabTools.InstantiatePrefabAsync(
                "Assets/InstantiatePrefabTest.prefab",
                ct);

            Assert.That(result.IsError, Is.Not.True,
                () => ((TextContentBlock)result.Content[0]).Text);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("InstantiatePrefabTest"));

            var response = JsonSerializer.Deserialize<InstantiatePrefabResponse>(text, s_jsonOptions);
            Assert.That(response, Is.Not.Null);
            Assert.That(response!.instanceId, Is.Not.EqualTo(0));
            instantiatedId = response.instanceId;
        }
        finally
        {
            if (instantiatedId != 0)
            {
                await _fixture.GameObjectTools.DeleteGameObjectAsync(instantiatedId, cancellationToken: ct);
            }

            await _fixture.GameObjectTools.DeleteGameObjectAsync(createResponse.instanceId, cancellationToken: ct);
            UnityEditorFixture.DeleteAssetFile("Assets/InstantiatePrefabTest.prefab");
        }
    }
}
