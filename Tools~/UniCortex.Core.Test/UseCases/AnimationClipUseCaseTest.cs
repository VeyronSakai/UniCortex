using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class AnimationClipUseCaseTest
{
    private const string TestAssetPath = "Assets/AnimationClipUseCaseTest.anim";
    private const string TransformType = "UnityEngine.Transform";
    private const string CoreModule = "UnityEngine.CoreModule";

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
    public async ValueTask Create_ReturnsSuccess_AndAppliesSettings()
    {
        var ct = CancellationToken.None;

        var json = await _fixture.AnimationClipUseCase.CreateAsync(TestAssetPath, true, 30f, ct);
        var response = JsonSerializer.Deserialize<CreateAnimationClipResponse>(json, s_jsonOptions);

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.success, Is.True);
        Assert.That(response.assetPath, Is.EqualTo(TestAssetPath));

        var curves = await GetCurvesAsync(ct);
        Assert.That(curves.frameRate, Is.EqualTo(30f));
        Assert.That(curves.loop, Is.True);
        Assert.That(curves.curves, Is.Empty);
    }

    [Test]
    public async ValueTask SetCurve_ThenGetCurves_ReturnsKeys()
    {
        var ct = CancellationToken.None;
        await _fixture.AnimationClipUseCase.CreateAsync(TestAssetPath, false, 60f, ct);

        var keys = AnimationClipUseCase.ParseKeys(
            """[{"time":0,"value":0},{"time":0.5,"value":1,"tangentMode":"Linear"}]""");
        var message = await _fixture.AnimationClipUseCase.SetCurveAsync(TestAssetPath, "Root/Child",
            TransformType, CoreModule, "m_LocalScale.x", keys, ct);
        Assert.That(message, Does.Contain("2 key(s)"));

        var curves = await GetCurvesAsync(ct);
        Assert.That(curves.curves, Has.Count.EqualTo(1));
        var curve = curves.curves[0];
        Assert.That(curve.path, Is.EqualTo("Root/Child"));
        Assert.That(curve.componentType, Is.EqualTo(TransformType));
        Assert.That(curve.assemblyName, Is.EqualTo(CoreModule));
        Assert.That(curve.propertyName, Is.EqualTo("m_LocalScale.x"));
        Assert.That(curve.keys, Has.Count.EqualTo(2));
        Assert.That(curve.keys[1].value, Is.EqualTo(1f));
        Assert.That(curve.keys[1].leftTangentMode, Is.EqualTo("Linear"));
    }

    [Test]
    public async ValueTask RemoveCurve_RemovesCurve()
    {
        var ct = CancellationToken.None;
        await _fixture.AnimationClipUseCase.CreateAsync(TestAssetPath, false, 60f, ct);
        var keys = new List<AnimationCurveKeyInput> { new() { time = 0f, value = 1f } };
        await _fixture.AnimationClipUseCase.SetCurveAsync(TestAssetPath, "", TransformType, CoreModule,
            "m_LocalScale.x", keys, ct);

        await _fixture.AnimationClipUseCase.RemoveCurveAsync(TestAssetPath, "", TransformType, CoreModule,
            "m_LocalScale.x", ct);

        var curves = await GetCurvesAsync(ct);
        Assert.That(curves.curves, Is.Empty);
    }

    [Test]
    public void SetCurve_Throws_WhenClipNotFound()
    {
        var keys = new List<AnimationCurveKeyInput> { new() { time = 0f, value = 1f } };

        Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.AnimationClipUseCase.SetCurveAsync("Assets/NotExisting.anim", "", TransformType,
                CoreModule, "m_LocalScale.x", keys, CancellationToken.None));
    }

    [Test]
    public void ParseKeys_IsCaseInsensitive()
    {
        var keys = AnimationClipUseCase.ParseKeys("""[{"Time":1.5,"VALUE":2,"tangentMode":"Auto"}]""");

        Assert.That(keys, Has.Count.EqualTo(1));
        Assert.That(keys[0].time, Is.EqualTo(1.5f));
        Assert.That(keys[0].value, Is.EqualTo(2f));
        Assert.That(keys[0].tangentMode, Is.EqualTo("Auto"));
    }

    private async ValueTask<GetAnimationCurvesResponse> GetCurvesAsync(CancellationToken ct)
    {
        var json = await _fixture.AnimationClipUseCase.GetCurvesAsync(TestAssetPath, ct);
        return JsonSerializer.Deserialize<GetAnimationCurvesResponse>(json, s_jsonOptions)!;
    }
}
