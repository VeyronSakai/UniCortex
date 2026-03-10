using System.Text.Json;
using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.GameObjectTools;

[TestFixture]
public class GameObjectToolsTest
{
    private const string TestScenePath = "Assets/Scenes/GameObjectToolsTestScene.unity";

    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
        await _fixture.EnsureNotInPlayModeAsync(CancellationToken.None);
        await _fixture.SceneTools.CreateSceneAsync(TestScenePath, CancellationToken.None);
        await _fixture.AssetTools.RefreshAssetDatabaseAsync(CancellationToken.None);
        await _fixture.SceneTools.SaveSceneAsync(CancellationToken.None);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        UnityEditorFixture.DeleteAssetFile(TestScenePath);
    }

    [SetUp]
    public async ValueTask SetUp()
    {
        await _fixture.SceneTools.CreateSceneAsync(TestScenePath, CancellationToken.None);
        await _fixture.AssetTools.RefreshAssetDatabaseAsync(CancellationToken.None);
        await _fixture.SceneTools.SaveSceneAsync(CancellationToken.None);
    }

    [TearDown]
    public async ValueTask TearDown()
    {
        await _fixture.SceneTools.OpenSceneAsync("Assets/Scenes/SampleScene.unity", CancellationToken.None);
        UnityEditorFixture.DeleteAssetFile(TestScenePath);
    }

    [Test]
    public async ValueTask FindGameObjects_ReturnsJsonWithGameObjects()
    {
        var tools = _fixture.GameObjectTools;

        var result = await tools.FindGameObjectsAsync(cancellationToken: CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("gameObjects"));
    }

    [Test]
    public async ValueTask FindGameObjects_WithQuery_ReturnsFilteredResults()
    {
        var tools = _fixture.GameObjectTools;

        var result = await tools.FindGameObjectsAsync(query: "t:Camera", cancellationToken: CancellationToken.None);

        Assert.That(result.IsError, Is.Not.True);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        var text = ((TextContentBlock)result.Content[0]).Text;
        Assert.That(text, Does.Contain("gameObjects"));
    }

    [Test]
    public async ValueTask CreateAndDeleteGameObject_WorksEndToEnd()
    {
        var tools = _fixture.GameObjectTools;

        var createResult = await tools.CreateGameObjectAsync("TestObj", cancellationToken: CancellationToken.None);

        Assert.That(createResult.IsError, Is.Not.True);
        var createText = ((TextContentBlock)createResult.Content[0]).Text;
        Assert.That(createText, Does.Contain("TestObj"));

        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createText, s_jsonOptions);
        Assert.That(createResponse, Is.Not.Null);

        var deleteResult =
            await tools.DeleteGameObjectAsync(createResponse!.instanceId, cancellationToken: CancellationToken.None);

        Assert.That(deleteResult.IsError, Is.Not.True);
        var deleteText = ((TextContentBlock)deleteResult.Content[0]).Text;
        Assert.That(deleteText, Does.Contain("deleted"));
    }

    [Test]
    public async ValueTask ModifyGameObject_ChangeName_ReturnsSuccess()
    {
        var tools = _fixture.GameObjectTools;
        var ct = CancellationToken.None;

        var createResult = await tools.CreateGameObjectAsync("ModifyNameTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await tools.ModifyGameObjectAsync(createResponse.instanceId, name: "RenamedObj",
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
            await tools.DeleteGameObjectAsync(createResponse.instanceId, cancellationToken: ct);
        }
    }

    [Test]
    public async ValueTask ModifyGameObject_ChangeActiveSelf_ReturnsSuccess()
    {
        var tools = _fixture.GameObjectTools;
        var ct = CancellationToken.None;

        var createResult = await tools.CreateGameObjectAsync("ModifyActiveTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await tools.ModifyGameObjectAsync(createResponse.instanceId, activeSelf: false,
                cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("modified successfully"));
        }
        finally
        {
            await tools.DeleteGameObjectAsync(createResponse.instanceId, cancellationToken: ct);
        }
    }

    [Test]
    public async ValueTask ModifyGameObject_ChangeTag_ReturnsSuccess()
    {
        var tools = _fixture.GameObjectTools;
        var ct = CancellationToken.None;

        var createResult = await tools.CreateGameObjectAsync("ModifyTagTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await tools.ModifyGameObjectAsync(createResponse.instanceId, tag: "EditorOnly",
                cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("modified successfully"));
        }
        finally
        {
            await tools.DeleteGameObjectAsync(createResponse.instanceId, cancellationToken: ct);
        }
    }

    [Test]
    public async ValueTask ModifyGameObject_ChangeLayer_ReturnsSuccess()
    {
        var tools = _fixture.GameObjectTools;
        var ct = CancellationToken.None;

        var createResult = await tools.CreateGameObjectAsync("ModifyLayerTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await tools.ModifyGameObjectAsync(createResponse.instanceId, layer: 1,
                cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("modified successfully"));
        }
        finally
        {
            await tools.DeleteGameObjectAsync(createResponse.instanceId, cancellationToken: ct);
        }
    }

    [Test]
    public async ValueTask ModifyGameObject_ChangeParent_ReturnsSuccess()
    {
        var tools = _fixture.GameObjectTools;
        var ct = CancellationToken.None;

        var createParentResult = await tools.CreateGameObjectAsync("ParentObj", cancellationToken: ct);
        var parentResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createParentResult.Content[0]).Text, s_jsonOptions)!;

        var createChildResult = await tools.CreateGameObjectAsync("ChildObj", cancellationToken: ct);
        var childResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createChildResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await tools.ModifyGameObjectAsync(childResponse.instanceId,
                parentInstanceId: parentResponse.instanceId, cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("modified successfully"));
        }
        finally
        {
            await tools.DeleteGameObjectAsync(childResponse.instanceId, cancellationToken: ct);
            await tools.DeleteGameObjectAsync(parentResponse.instanceId, cancellationToken: ct);
        }
    }

    [Test]
    public async ValueTask ModifyGameObject_MultipleProperties_ReturnsSuccess()
    {
        var tools = _fixture.GameObjectTools;
        var ct = CancellationToken.None;

        var createResult = await tools.CreateGameObjectAsync("ModifyMultiTest", cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await tools.ModifyGameObjectAsync(createResponse.instanceId,
                name: "MultiModified", activeSelf: false, tag: "EditorOnly", layer: 1,
                cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("modified successfully"));
        }
        finally
        {
            await tools.DeleteGameObjectAsync(createResponse.instanceId, cancellationToken: ct);
        }
    }
}
