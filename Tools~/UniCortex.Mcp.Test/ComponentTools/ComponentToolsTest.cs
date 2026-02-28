using System.Text.Json;
using NUnit.Framework;
using ModelContextProtocol.Protocol;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Test.Fixtures;

namespace UniCortex.Mcp.Test.ComponentTools;

[TestFixture]
public class ComponentToolsTest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async Task AddComponent_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createResult = await _fixture.GameObjectTools.CreateGameObject("AddComponentTestObj",
            cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await _fixture.ComponentTools.AddComponent(
                instanceId: createResponse.instanceId,
                componentType: "UnityEngine.Rigidbody",
                cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("added successfully"));
        }
        finally
        {
            await _fixture.GameObjectTools.DeleteGameObject(createResponse.instanceId, cancellationToken: ct);
        }
    }

    [Test]
    public async Task RemoveComponent_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createResult = await _fixture.GameObjectTools.CreateGameObject("RemoveComponentTestObj",
            cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            await _fixture.ComponentTools.AddComponent(
                instanceId: createResponse.instanceId,
                componentType: "UnityEngine.Rigidbody",
                cancellationToken: ct);

            var result = await _fixture.ComponentTools.RemoveComponent(
                instanceId: createResponse.instanceId,
                componentType: "UnityEngine.Rigidbody",
                cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("removed successfully"));
        }
        finally
        {
            await _fixture.GameObjectTools.DeleteGameObject(createResponse.instanceId, cancellationToken: ct);
        }
    }

    [Test]
    public async Task GetComponentProperties_ReturnsJsonWithProperties()
    {
        var ct = CancellationToken.None;

        var createResult = await _fixture.GameObjectTools.CreateGameObject("GetPropertiesTestObj",
            cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await _fixture.ComponentTools.GetComponentProperties(
                instanceId: createResponse.instanceId,
                componentType: "UnityEngine.Transform",
                cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("UnityEngine.Transform"));
            Assert.That(text, Does.Contain("m_LocalPosition"));
        }
        finally
        {
            await _fixture.GameObjectTools.DeleteGameObject(createResponse.instanceId, cancellationToken: ct);
        }
    }

    [Test]
    public async Task SetComponentProperty_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createResult = await _fixture.GameObjectTools.CreateGameObject("SetPropertyTestObj",
            cancellationToken: ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(
            ((TextContentBlock)createResult.Content[0]).Text, s_jsonOptions)!;

        try
        {
            var result = await _fixture.ComponentTools.SetComponentProperty(
                instanceId: createResponse.instanceId,
                componentType: "UnityEngine.Transform",
                propertyPath: "m_LocalPosition.x",
                value: "1.5",
                cancellationToken: ct);

            Assert.That(result.IsError, Is.Not.True);
            var text = ((TextContentBlock)result.Content[0]).Text;
            Assert.That(text, Does.Contain("Property 'm_LocalPosition.x' set to '1.5' successfully."));
        }
        finally
        {
            await _fixture.GameObjectTools.DeleteGameObject(createResponse.instanceId, cancellationToken: ct);
        }
    }
}
