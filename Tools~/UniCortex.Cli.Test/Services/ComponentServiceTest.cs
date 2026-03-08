using System.Text.Json;
using NUnit.Framework;
using UniCortex.Cli.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Test.Services;

[TestFixture]
public class ComponentServiceTest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [Test]
    public async ValueTask Add_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectService.CreateAsync("AddComponentTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.ComponentService.AddAsync(
                createResponse.instanceId, "UnityEngine.Rigidbody", ct);

            Assert.That(message, Does.Contain("added successfully"));
        }
        finally
        {
            await _fixture.GameObjectService.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Remove_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectService.CreateAsync("RemoveComponentTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            await _fixture.ComponentService.AddAsync(
                createResponse.instanceId, "UnityEngine.Rigidbody", ct);

            var message = await _fixture.ComponentService.RemoveAsync(
                createResponse.instanceId, "UnityEngine.Rigidbody", cancellationToken: ct);

            Assert.That(message, Does.Contain("removed successfully"));
        }
        finally
        {
            await _fixture.GameObjectService.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask GetProperties_ReturnsJsonWithProperties()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectService.CreateAsync("GetPropertiesTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var json = await _fixture.ComponentService.GetPropertiesAsync(
                createResponse.instanceId, "UnityEngine.Transform", cancellationToken: ct);

            Assert.That(json, Does.Contain("UnityEngine.Transform"));
            Assert.That(json, Does.Contain("m_LocalPosition"));
        }
        finally
        {
            await _fixture.GameObjectService.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask SetProperty_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectService.CreateAsync("SetPropertyTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.ComponentService.SetPropertyAsync(
                createResponse.instanceId, "UnityEngine.Transform",
                "m_LocalPosition.x", "1.5", ct);

            Assert.That(message, Does.Contain("Property 'm_LocalPosition.x' set to '1.5' successfully."));
        }
        finally
        {
            await _fixture.GameObjectService.DeleteAsync(createResponse.instanceId, ct);
        }
    }
}
