using System.Text.Json;
using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Test.Services;

[TestFixture]
public class GameObjectServiceTest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask Find_ReturnsJsonWithGameObjects()
    {
        var result = await _fixture.GameObjectService.FindAsync(null, CancellationToken.None);

        Assert.That(result, Does.Contain("gameObjects"));
    }

    [Test]
    public async ValueTask Find_WithQuery_ReturnsFilteredResults()
    {
        var result = await _fixture.GameObjectService.FindAsync("t:Camera", CancellationToken.None);

        Assert.That(result, Does.Contain("gameObjects"));
    }

    [Test]
    public async ValueTask CreateAndDelete_WorksEndToEnd()
    {
        var createJson = await _fixture.GameObjectService.CreateAsync("TestObj", CancellationToken.None);

        Assert.That(createJson, Does.Contain("TestObj"));

        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions);
        Assert.That(createResponse, Is.Not.Null);

        var deleteMessage =
            await _fixture.GameObjectService.DeleteAsync(createResponse!.instanceId, CancellationToken.None);

        Assert.That(deleteMessage, Does.Contain("deleted"));
    }

    [Test]
    public async ValueTask Modify_ChangeName_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectService.CreateAsync("ModifyNameTest", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.GameObjectService.ModifyAsync(createResponse.instanceId,
                name: "RenamedObj", cancellationToken: ct);

            Assert.That(message, Does.Contain("modified successfully"));
        }
        finally
        {
            await _fixture.GameObjectService.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Modify_ChangeActiveSelf_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectService.CreateAsync("ModifyActiveTest", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.GameObjectService.ModifyAsync(createResponse.instanceId,
                activeSelf: false, cancellationToken: ct);

            Assert.That(message, Does.Contain("modified successfully"));
        }
        finally
        {
            await _fixture.GameObjectService.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Modify_ChangeTag_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectService.CreateAsync("ModifyTagTest", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.GameObjectService.ModifyAsync(createResponse.instanceId,
                tag: "EditorOnly", cancellationToken: ct);

            Assert.That(message, Does.Contain("modified successfully"));
        }
        finally
        {
            await _fixture.GameObjectService.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Modify_ChangeLayer_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectService.CreateAsync("ModifyLayerTest", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.GameObjectService.ModifyAsync(createResponse.instanceId,
                layer: 1, cancellationToken: ct);

            Assert.That(message, Does.Contain("modified successfully"));
        }
        finally
        {
            await _fixture.GameObjectService.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Modify_ChangeParent_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var parentJson = await _fixture.GameObjectService.CreateAsync("ParentObj", ct);
        var parentResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(parentJson, s_jsonOptions)!;

        var childJson = await _fixture.GameObjectService.CreateAsync("ChildObj", ct);
        var childResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(childJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.GameObjectService.ModifyAsync(childResponse.instanceId,
                parentInstanceId: parentResponse.instanceId, cancellationToken: ct);

            Assert.That(message, Does.Contain("modified successfully"));
        }
        finally
        {
            await _fixture.GameObjectService.DeleteAsync(childResponse.instanceId, ct);
            await _fixture.GameObjectService.DeleteAsync(parentResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Modify_MultipleProperties_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectService.CreateAsync("ModifyMultiTest", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.GameObjectService.ModifyAsync(createResponse.instanceId,
                name: "MultiModified", activeSelf: false, tag: "EditorOnly", layer: 1,
                cancellationToken: ct);

            Assert.That(message, Does.Contain("modified successfully"));
        }
        finally
        {
            await _fixture.GameObjectService.DeleteAsync(createResponse.instanceId, ct);
        }
    }
}
