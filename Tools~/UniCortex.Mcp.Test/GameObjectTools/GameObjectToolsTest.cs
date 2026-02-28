using System.Text.Json;
using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.GameObjectTools;

[TestFixture]
public class GameObjectToolsTest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async Task GetGameObjects_ReturnsJsonWithGameObjects()
    {
        var tools = _fixture.GameObjectTools;

        var result = await tools.GetGameObjects(cancellationToken: CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("gameObjects"));
    }

    [Test]
    public async Task GetGameObjects_WithQuery_ReturnsFilteredResults()
    {
        var tools = _fixture.GameObjectTools;

        var result = await tools.GetGameObjects(query: "t:Camera", cancellationToken: CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("gameObjects"));
    }

    [Test]
    public async Task CreateAndDeleteGameObject_WorksEndToEnd()
    {
        var tools = _fixture.GameObjectTools;

        var createResult = await tools.CreateGameObject("TestObj", cancellationToken: CancellationToken.None);

        Assert.That(createResult.IsError, Is.Not.True);
        var createText = ((TextContentBlock)createResult.Content[0]).Text;
        Assert.That(createText, Does.Contain("TestObj"));

        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createText, s_jsonOptions);
        Assert.That(createResponse, Is.Not.Null);

        var deleteResult =
            await tools.DeleteGameObject(createResponse!.instanceId, cancellationToken: CancellationToken.None);

        Assert.That(deleteResult.IsError, Is.Not.True);
        var deleteText = ((TextContentBlock)deleteResult.Content[0]).Text;
        Assert.That(deleteText, Does.Contain("deleted"));
    }

    [Test]
    public async Task ModifyGameObject_ChangeName_ReturnsSuccess()
    {
        var tools = _fixture.GameObjectTools;
        var ct = CancellationToken.None;

        var createResult = await tools.CreateGameObject("ModifyNameTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await tools.ModifyGameObject(createResponse.instanceId, name: "RenamedObj",
                cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("modified successfully"));
        }
        catch (Exception ex)
        {
            Assert.Fail($"Unexpected exception: {ex}");
        }
        finally
        {
            await tools.DeleteGameObject(createResponse.instanceId, cancellationToken: ct);
        }
    }

    [Test]
    public async Task ModifyGameObject_ChangeActiveSelf_ReturnsSuccess()
    {
        var tools = _fixture.GameObjectTools;
        var ct = CancellationToken.None;

        var createResult = await tools.CreateGameObject("ModifyActiveTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await tools.ModifyGameObject(createResponse.instanceId, activeSelf: false,
                cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("modified successfully"));
        }
        finally
        {
            await tools.DeleteGameObject(createResponse.instanceId, cancellationToken: ct);
        }
    }

    [Test]
    public async Task ModifyGameObject_ChangeTag_ReturnsSuccess()
    {
        var tools = _fixture.GameObjectTools;
        var ct = CancellationToken.None;

        var createResult = await tools.CreateGameObject("ModifyTagTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await tools.ModifyGameObject(createResponse.instanceId, tag: "EditorOnly",
                cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("modified successfully"));
        }
        finally
        {
            await tools.DeleteGameObject(createResponse.instanceId, cancellationToken: ct);
        }
    }

    [Test]
    public async Task ModifyGameObject_ChangeLayer_ReturnsSuccess()
    {
        var tools = _fixture.GameObjectTools;
        var ct = CancellationToken.None;

        var createResult = await tools.CreateGameObject("ModifyLayerTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await tools.ModifyGameObject(createResponse.instanceId, layer: 1,
                cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("modified successfully"));
        }
        finally
        {
            await tools.DeleteGameObject(createResponse.instanceId, cancellationToken: ct);
        }
    }

    [Test]
    public async Task ModifyGameObject_ChangeParent_ReturnsSuccess()
    {
        var tools = _fixture.GameObjectTools;
        var ct = CancellationToken.None;

        var createParentResult = await tools.CreateGameObject("ParentObj", cancellationToken: ct);
        var parentResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createParentResult.Content[0]).Text, s_jsonOptions)!;

        var createChildResult = await tools.CreateGameObject("ChildObj", cancellationToken: ct);
        var childResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createChildResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await tools.ModifyGameObject(childResponse.instanceId,
                parentInstanceId: parentResponse.instanceId, cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("modified successfully"));
        }
        finally
        {
            await tools.DeleteGameObject(childResponse.instanceId, cancellationToken: ct);
            await tools.DeleteGameObject(parentResponse.instanceId, cancellationToken: ct);
        }
    }

    [Test]
    public async Task ModifyGameObject_MultipleProperties_ReturnsSuccess()
    {
        var tools = _fixture.GameObjectTools;
        var ct = CancellationToken.None;

        var createResult = await tools.CreateGameObject("ModifyMultiTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await tools.ModifyGameObject(createResponse.instanceId,
                name: "MultiModified", activeSelf: false, tag: "EditorOnly", layer: 1,
                cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("modified successfully"));
        }
        finally
        {
            await tools.DeleteGameObject(createResponse.instanceId, cancellationToken: ct);
        }
    }
}
