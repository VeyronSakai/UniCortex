using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class ScriptableObjectUseCaseTest
{
    // UniCortex.Editor.Tests assembly is loaded in the Samples~ project via the
    // "testables" entry in Packages/manifest.json (UNITY_INCLUDE_TESTS defined).
    private const string TestTypeName =
        "UniCortex.Editor.Tests.TestDoubles.AllPropertyTypesScriptableObject";

    private const string TestAssetPath = "Assets/ScriptableObjectUseCaseTest.asset";

    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private UnityEditorFixture _fixture = null!;

    [OneTimeSetUp]
    public async ValueTask OneTimeSetUp()
    {
        _fixture = await UnityEditorFixture.CreateAsync();
    }

    [TearDown]
    public async ValueTask TearDown()
    {
        UnityEditorFixture.DeleteAssetFile(TestAssetPath);
        await _fixture.AssetUseCase.RefreshAsync(CancellationToken.None);
    }

    [Test]
    public async ValueTask Create_ReturnsSuccess_AndAssignsInstanceId()
    {
        var ct = CancellationToken.None;

        var json = await _fixture.ScriptableObjectUseCase.CreateAsync(TestTypeName, TestAssetPath, ct);
        var response = JsonSerializer.Deserialize<CreateScriptableObjectResponse>(json, s_jsonOptions);

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.success, Is.True);
        Assert.That(response.instanceId, Is.Not.EqualTo(0));
    }

    [Test]
    public async ValueTask GetProperties_ReturnsPropertyList_AfterCreate()
    {
        var ct = CancellationToken.None;

        await _fixture.ScriptableObjectUseCase.CreateAsync(TestTypeName, TestAssetPath, ct);

        var json = await _fixture.ScriptableObjectUseCase.GetPropertiesAsync(TestAssetPath, ct);

        Assert.That(json, Does.Contain("AllPropertyTypesScriptableObject"));
        Assert.That(json, Does.Contain("intField"));
        Assert.That(json, Does.Contain("floatField"));
    }

    [Test]
    public async ValueTask SetProperty_PersistsValue_AndIsReadableAgain()
    {
        var ct = CancellationToken.None;

        await _fixture.ScriptableObjectUseCase.CreateAsync(TestTypeName, TestAssetPath, ct);

        var message = await _fixture.ScriptableObjectUseCase.SetPropertyAsync(
            TestAssetPath, "intField", "42", ct);
        Assert.That(message, Does.Contain("set to '42' successfully"));

        var json = await _fixture.ScriptableObjectUseCase.GetPropertiesAsync(TestAssetPath, ct);
        var response =
            JsonSerializer.Deserialize<GetScriptableObjectPropertiesResponse>(json, s_jsonOptions)!;

        var intField = response.properties.Find(p => p.path == "intField");
        Assert.That(intField, Is.Not.Null);
        Assert.That(intField!.value, Is.EqualTo("42"));
    }
}
