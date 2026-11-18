using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Domains;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class AudioMixerUseCaseTest
{
    private const string TestAssetPath = "Assets/AudioMixerUseCaseTest.mixer";

    private static readonly JsonSerializerOptions s_jsonOptions = JsonOptions.Default;
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

    private async ValueTask<GetAudioMixerResponse> GetMixerAsync(CancellationToken ct)
    {
        var json = await _fixture.AudioMixerUseCase.GetAsync(TestAssetPath, ct);
        return JsonSerializer.Deserialize<GetAudioMixerResponse>(json, s_jsonOptions)!;
    }

    [Test]
    public async ValueTask Create_ThenGet_ReturnsMasterGroupAndSnapshot()
    {
        // Arrange
        var ct = CancellationToken.None;

        // Act
        var json = await _fixture.AudioMixerUseCase.CreateAsync(TestAssetPath, ct);

        // Assert
        var response = JsonSerializer.Deserialize<CreateAudioMixerResponse>(json, s_jsonOptions)!;
        Assert.That(response.success, Is.True);
        var mixer = await GetMixerAsync(ct);
        Assert.That(mixer.groups.Select(g => g.path), Is.EqualTo(new[] { "Master" }));
        Assert.That(mixer.snapshots, Has.Count.EqualTo(1));
        Assert.That(mixer.exposedParameters, Is.Empty);
    }

    [Test]
    public async ValueTask AddGroup_ThenExposeVolume_ReturnsGroupsAndExposedParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        await _fixture.AudioMixerUseCase.CreateAsync(TestAssetPath, ct);

        // Act
        var addMusic = await _fixture.AudioMixerUseCase.AddGroupAsync(TestAssetPath, "Music", null, ct);
        await _fixture.AudioMixerUseCase.AddGroupAsync(TestAssetPath, "Effect", "Master", ct);
        await _fixture.AudioMixerUseCase.ExposeParameterAsync(TestAssetPath, "Music", "volume", "MusicVolume", ct);
        await _fixture.AudioMixerUseCase.ExposeParameterAsync(TestAssetPath, "Master/Effect", "volume",
            "EffectVolume", ct);

        // Assert
        Assert.That(addMusic, Does.Contain("Master/Music"));
        var mixer = await GetMixerAsync(ct);
        Assert.That(mixer.groups.Select(g => g.path),
            Is.EqualTo(new[] { "Master", "Master/Music", "Master/Effect" }));
        Assert.That(mixer.exposedParameters.Select(p => (p.name, p.groupPath, p.parameter)),
            Is.EquivalentTo(new[]
            {
                ("MusicVolume", "Master/Music", "volume"),
                ("EffectVolume", "Master/Effect", "volume")
            }));
    }

    [Test]
    public async ValueTask AddGroup_WithUnknownParent_ReturnsError()
    {
        // Arrange
        var ct = CancellationToken.None;
        await _fixture.AudioMixerUseCase.CreateAsync(TestAssetPath, ct);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.AudioMixerUseCase.AddGroupAsync(TestAssetPath, "Music", "Missing", ct));
    }

    [Test]
    public async ValueTask SetAudioSourceOutput_RoutesAudioSourceToGroup()
    {
        // Arrange
        var ct = CancellationToken.None;
        await _fixture.AudioMixerUseCase.CreateAsync(TestAssetPath, ct);
        await _fixture.AudioMixerUseCase.AddGroupAsync(TestAssetPath, "Music", null, ct);
        var createJson = await _fixture.GameObjectUseCase.CreateAsync("AudioMixerUseCaseTestSource",
            cancellationToken: ct);
        var go = JsonSerializer.Deserialize<CreateGameObjectResponse>(createJson, s_jsonOptions)!;

        try
        {
            await _fixture.ComponentUseCase.AddAsync(go.instanceId, "UnityEngine.AudioSource",
                "UnityEngine.AudioModule", ct);

            // Act
            var message = await _fixture.AudioMixerUseCase.SetAudioSourceOutputAsync(go.instanceId,
                TestAssetPath, "Master/Music", cancellationToken: ct);

            // Assert
            Assert.That(message, Does.Contain("Master/Music"));
            var properties = await _fixture.ComponentUseCase.GetPropertiesAsync(go.instanceId,
                "UnityEngine.AudioSource", "UnityEngine.AudioModule", cancellationToken: ct);
            // Sub-asset references (the mixer group) are reported with the path of the asset that holds them.
            Assert.That(properties,
                Does.Contain($"{{\"path\":\"OutputAudioMixerGroup\",\"type\":\"ObjectReference\",\"value\":\"{TestAssetPath}\"}}"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(go.instanceId, ct);
        }
    }
}
