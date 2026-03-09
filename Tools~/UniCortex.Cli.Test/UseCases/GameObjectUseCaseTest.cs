using System.Text.Json;
using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Test.UseCases;

[TestFixture]
public class GameObjectUseCaseTest
{
    private const string TestScenePath = "Assets/Scenes/GameObjectUseCaseTestScene.unity";

    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
        await _fixture.EnsureNotInPlayModeAsync(CancellationToken.None);
        await _fixture.SceneUseCase.CreateAsync(TestScenePath, CancellationToken.None);
        await _fixture.AssetUseCase.RefreshAsync(CancellationToken.None);
        await _fixture.SceneUseCase.SaveAsync(CancellationToken.None);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        UnityEditorFixture.DeleteAssetFile(TestScenePath);
    }

    [Test]
    public async ValueTask Find_ReturnsJsonWithGameObjects()
    {
        var result = await _fixture.GameObjectUseCase.FindAsync(null, CancellationToken.None);

        Assert.That(result, Does.Contain("gameObjects"));
    }

    [Test]
    public async ValueTask Find_WithQuery_ReturnsFilteredResults()
    {
        var result = await _fixture.GameObjectUseCase.FindAsync("t:Camera", CancellationToken.None);

        Assert.That(result, Does.Contain("gameObjects"));
    }

    [Test]
    public async ValueTask CreateAndDelete_WorksEndToEnd()
    {
        var createJson = await _fixture.GameObjectUseCase.CreateAsync("TestObj", CancellationToken.None);

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

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("ModifyNameTest", ct);
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

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("ModifyActiveTest", ct);
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

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("ModifyTagTest", ct);
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

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("ModifyLayerTest", ct);
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

        var parentJson = await _fixture.GameObjectUseCase.CreateAsync("ParentObj", ct);
        var parentResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(parentJson, s_jsonOptions)!;

        var childJson = await _fixture.GameObjectUseCase.CreateAsync("ChildObj", ct);
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

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("ModifyMultiTest", ct);
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
}
