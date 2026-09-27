using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class ComponentUseCaseTest
{
    private const string TestScenePath = "Assets/Scenes/ComponentToolsTestScene.unity";

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
        await _fixture.EditorUseCase.SaveAsync(CancellationToken.None);
    }

    [TearDown]
    public async ValueTask TearDown()
    {
        // Open SampleScene first so the test scene is unloaded from Unity.
        // OpenScene calls SaveIfDirty(), which would recreate the file if we deleted it first.
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        UnityEditorFixture.DeleteAssetFile(TestScenePath);
    }

    [Test]
    public async ValueTask Add_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("AddComponentTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.ComponentUseCase.AddAsync(
                createResponse.instanceId, "UnityEngine.Rigidbody", "UnityEngine.PhysicsModule", ct);

            Assert.That(message, Does.Contain("added successfully"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask Remove_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("RemoveComponentTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            await _fixture.ComponentUseCase.AddAsync(
                createResponse.instanceId, "UnityEngine.Rigidbody", "UnityEngine.PhysicsModule", ct);

            var message = await _fixture.ComponentUseCase.RemoveAsync(
                createResponse.instanceId, "UnityEngine.Rigidbody", "UnityEngine.PhysicsModule",
                cancellationToken: ct);

            Assert.That(message, Does.Contain("removed successfully"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask GetProperties_ReturnsJsonWithProperties()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("GetPropertiesTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var json = await _fixture.ComponentUseCase.GetPropertiesAsync(
                createResponse.instanceId, "UnityEngine.Transform", "UnityEngine.CoreModule",
                cancellationToken: ct);

            Assert.That(json, Does.Contain("UnityEngine.Transform"));
            Assert.That(json, Does.Contain("m_LocalPosition"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask SetProperty_ReturnsSuccess()
    {
        var ct = CancellationToken.None;

        var createJson = await _fixture.GameObjectUseCase.CreateAsync("SetPropertyTestObj", ct);
        var createResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            var message = await _fixture.ComponentUseCase.SetPropertyAsync(
                createResponse.instanceId, "UnityEngine.Transform", "UnityEngine.CoreModule",
                "m_LocalPosition.x", "1.5", ct);

            Assert.That(message, Does.Contain("Property 'm_LocalPosition.x' set to '1.5' successfully."));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(createResponse.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask SetProperty_GameObjectInstanceIdForComponentReference_AssignsMatchingComponent()
    {
        var ct = CancellationToken.None;

        // Arrange
        var canvasJson = await _fixture.GameObjectUseCase.CreateAsync("CanvasObj", ct);
        var canvas = JsonSerializer.Deserialize<CreateGameObjectResponse>(canvasJson, s_jsonOptions)!;
        var cameraJson = await _fixture.GameObjectUseCase.CreateAsync("CameraObj", ct);
        var camera = JsonSerializer.Deserialize<CreateGameObjectResponse>(cameraJson, s_jsonOptions)!;

        try
        {
            await _fixture.ComponentUseCase.AddAsync(canvas.instanceId,
                "UnityEngine.Canvas", "UnityEngine.UIModule", ct);
            await _fixture.ComponentUseCase.AddAsync(camera.instanceId,
                "UnityEngine.Camera", "UnityEngine.CoreModule", ct);

            // Act
            await _fixture.ComponentUseCase.SetPropertyAsync(canvas.instanceId,
                "UnityEngine.Canvas", "UnityEngine.UIModule",
                "m_Camera", camera.instanceId.ToString(), ct);

            // Assert
            var json = await _fixture.ComponentUseCase.GetPropertiesAsync(canvas.instanceId,
                "UnityEngine.Canvas", "UnityEngine.UIModule", cancellationToken: ct);
            var response = JsonSerializer.Deserialize<GetComponentPropertiesResponse>(json, s_jsonOptions)!;
            var cameraReference = response.properties.Single(p => p.path == "m_Camera").value;
            Assert.That(cameraReference, Is.Not.EqualTo("null"));
            Assert.That(cameraReference, Is.Not.EqualTo(camera.instanceId.ToString()));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(canvas.instanceId, ct);
            await _fixture.GameObjectUseCase.DeleteAsync(camera.instanceId, ct);
        }
    }

    [Test]
    public async ValueTask SetProperty_ObjectReferenceTypeMismatch_Throws()
    {
        var ct = CancellationToken.None;

        // Arrange
        var canvasJson = await _fixture.GameObjectUseCase.CreateAsync("MismatchCanvasObj", ct);
        var canvas = JsonSerializer.Deserialize<CreateGameObjectResponse>(canvasJson, s_jsonOptions)!;

        try
        {
            await _fixture.ComponentUseCase.AddAsync(canvas.instanceId,
                "UnityEngine.Canvas", "UnityEngine.UIModule", ct);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.ComponentUseCase.SetPropertyAsync(canvas.instanceId,
                    "UnityEngine.Canvas", "UnityEngine.UIModule",
                    "m_Camera", TestScenePath, ct));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(canvas.instanceId, ct);
        }
    }
}
