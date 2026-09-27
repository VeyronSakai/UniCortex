using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class TimelineUseCaseTest
{
    private const string TestScenePath = "Assets/Scenes/TimelineToolsTestScene.unity";
    private const string TimelineAssetPath = "Assets/TimelineToolsTest.playable";
    private const string AnimationClipPath = "Assets/TimelineToolsTest.anim";

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
        await _fixture.SceneUseCase.OpenAsync(TestConstants.SampleScenePath, CancellationToken.None);
        UnityEditorFixture.DeleteAssetFile(TestScenePath);
        UnityEditorFixture.DeleteAssetFile(TimelineAssetPath);
        UnityEditorFixture.DeleteAssetFile(AnimationClipPath);
    }

    /// <summary>
    /// Creates a TimelineAsset, a GameObject with a PlayableDirector, and assigns the asset.
    /// Returns the GameObject's instanceId.
    /// </summary>
    private async ValueTask<int> CreateTimelineSetupAsync(CancellationToken cancellationToken)
    {
        // Create the .playable asset
        await _fixture.TimelineUseCase.CreateAsync(TimelineAssetPath, cancellationToken);

        // Create a GameObject
        var goJson = await _fixture.GameObjectUseCase.CreateAsync("TimelineTestObj", cancellationToken: cancellationToken);
        var goResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(goJson, s_jsonOptions)!;

        // Add PlayableDirector component
        await _fixture.ComponentUseCase.AddAsync(goResponse.instanceId,
            "UnityEngine.Playables.PlayableDirector", "UnityEngine.DirectorModule", cancellationToken);

        // Assign the TimelineAsset to PlayableDirector via set_component_property
        await _fixture.ComponentUseCase.SetPropertyAsync(goResponse.instanceId,
            "UnityEngine.Playables.PlayableDirector", "UnityEngine.DirectorModule",
            "m_PlayableAsset", TimelineAssetPath, cancellationToken);

        return goResponse.instanceId;
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SetComponentProperty_AssignsTimelineByGuid(CancellationToken cancellationToken)
    {
        // Arrange
        await _fixture.TimelineUseCase.CreateAsync(TimelineAssetPath, cancellationToken);
        var guid = UnityEditorFixture.ReadAssetGuid(TimelineAssetPath);
        var goJson = await _fixture.GameObjectUseCase.CreateAsync("TimelineGuidObj", cancellationToken: cancellationToken);
        var go = JsonSerializer.Deserialize<CreateGameObjectResponse>(goJson, s_jsonOptions)!;

        try
        {
            await _fixture.ComponentUseCase.AddAsync(go.instanceId,
                "UnityEngine.Playables.PlayableDirector", "UnityEngine.DirectorModule", cancellationToken);

            // Act
            await _fixture.ComponentUseCase.SetPropertyAsync(go.instanceId,
                "UnityEngine.Playables.PlayableDirector", "UnityEngine.DirectorModule",
                "m_PlayableAsset", guid, cancellationToken);

            // Assert
            var json = await _fixture.ComponentUseCase.GetPropertiesAsync(go.instanceId,
                "UnityEngine.Playables.PlayableDirector", "UnityEngine.DirectorModule",
                cancellationToken: cancellationToken);
            var response = JsonSerializer.Deserialize<GetComponentPropertiesResponse>(json, s_jsonOptions)!;
            Assert.That(response.properties.Single(p => p.path == "m_PlayableAsset").value,
                Is.EqualTo(TimelineAssetPath));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(go.instanceId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Create_CreatesTimelineAsset(CancellationToken cancellationToken)
    {
        var json = await _fixture.TimelineUseCase.CreateAsync(TimelineAssetPath, cancellationToken);

        Assert.That(json, Does.Contain(TimelineAssetPath));
        Assert.That(json, Does.Contain("true"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask AddTrack_AddsTrackToTimeline(CancellationToken cancellationToken)
    {
        var goId = await CreateTimelineSetupAsync(cancellationToken);

        try
        {
            var message = await _fixture.TimelineUseCase.AddTrackAsync(
                goId, "UnityEngine.Timeline.ActivationTrack", "TestTrack", cancellationToken);

            Assert.That(message, Does.Contain("Track added"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask RemoveTrack_RemovesTrackFromTimeline(CancellationToken cancellationToken)
    {
        var goId = await CreateTimelineSetupAsync(cancellationToken);

        try
        {
            await _fixture.TimelineUseCase.AddTrackAsync(
                goId, "UnityEngine.Timeline.ActivationTrack", "TrackToRemove", cancellationToken);

            var message = await _fixture.TimelineUseCase.RemoveTrackAsync(goId, 0, cancellationToken);

            Assert.That(message, Does.Contain("Track removed"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask AddClip_AddsClipToTrack(CancellationToken cancellationToken)
    {
        var goId = await CreateTimelineSetupAsync(cancellationToken);

        try
        {
            await _fixture.TimelineUseCase.AddTrackAsync(
                goId, "UnityEngine.Timeline.ActivationTrack", "ClipTestTrack", cancellationToken);

            var message = await _fixture.TimelineUseCase.AddClipAsync(goId, 0, 1.0, 3.0, "TestClip",
                cancellationToken);

            Assert.That(message, Does.Contain("Clip added"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask RemoveClip_RemovesClipFromTrack(CancellationToken cancellationToken)
    {
        var goId = await CreateTimelineSetupAsync(cancellationToken);

        try
        {
            await _fixture.TimelineUseCase.AddTrackAsync(
                goId, "UnityEngine.Timeline.ActivationTrack", "ClipRemoveTrack", cancellationToken);
            await _fixture.TimelineUseCase.AddClipAsync(goId, 0, 0, 5.0, "ClipToRemove", cancellationToken);

            var message = await _fixture.TimelineUseCase.RemoveClipAsync(goId, 0, 0, cancellationToken);

            Assert.That(message, Does.Contain("Clip removed"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Play_PlaysTimeline(CancellationToken cancellationToken)
    {
        var goId = await CreateTimelineSetupAsync(cancellationToken);

        try
        {
            var message = await _fixture.TimelineUseCase.PlayAsync(goId, cancellationToken);

            Assert.That(message, Does.Contain("playback started"));
        }
        finally
        {
            await _fixture.TimelineUseCase.StopAsync(goId, cancellationToken);
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Stop_StopsTimeline(CancellationToken cancellationToken)
    {
        var goId = await CreateTimelineSetupAsync(cancellationToken);

        try
        {
            await _fixture.TimelineUseCase.PlayAsync(goId, cancellationToken);

            var message = await _fixture.TimelineUseCase.StopAsync(goId, cancellationToken);

            Assert.That(message, Does.Contain("playback stopped"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Evaluate_EvaluatesTimelineAtTime(CancellationToken cancellationToken)
    {
        // Arrange
        var goId = await CreateTimelineSetupAsync(cancellationToken);

        try
        {
            // Act
            var message = await _fixture.TimelineUseCase.EvaluateAsync(goId, 0.5, cancellationToken);

            // Assert
            Assert.That(message, Does.Contain("evaluated at 0.5"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public void Evaluate_Throws_WhenTimeIsNegative(CancellationToken cancellationToken)
    {
        // Arrange & Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.TimelineUseCase.EvaluateAsync(1, -1.0, cancellationToken));
    }

    /// <summary>
    /// Creates the timeline setup with one Animation track (bound to the director's GameObject)
    /// and one clip on it. Returns the GameObject's instanceId.
    /// </summary>
    private async ValueTask<int> CreateAnimationClipSetupAsync(CancellationToken cancellationToken)
    {
        var goId = await CreateTimelineSetupAsync(cancellationToken);
        await _fixture.TimelineUseCase.AddTrackAsync(
            goId, "UnityEngine.Timeline.AnimationTrack", "AnimTrack", cancellationToken);
        await _fixture.TimelineUseCase.AddClipAsync(goId, 0, 1.0, 2.0, "AnimClip", cancellationToken);
        await _fixture.TimelineUseCase.BindTrackAsync(goId, 0, goId, cancellationToken);
        return goId;
    }

    private async ValueTask<TimelineClipEntry> GetFirstClipAsync(int goId, CancellationToken cancellationToken)
    {
        var json = await _fixture.TimelineUseCase.GetTracksAsync(goId, null, cancellationToken);
        var response = JsonSerializer.Deserialize<GetTimelineTracksResponse>(json, s_jsonOptions)!;
        return response.tracks[0].clips[0];
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetTracks_ReturnsTracksAndClips(CancellationToken cancellationToken)
    {
        // Arrange
        var goId = await CreateAnimationClipSetupAsync(cancellationToken);

        try
        {
            // Act
            var json = await _fixture.TimelineUseCase.GetTracksAsync(goId, null, cancellationToken);

            // Assert
            var response = JsonSerializer.Deserialize<GetTimelineTracksResponse>(json, s_jsonOptions)!;
            Assert.That(response.assetPath, Is.EqualTo(TimelineAssetPath));
            Assert.That(response.tracks, Has.Count.EqualTo(1));
            var track = response.tracks[0];
            Assert.That(track.name, Is.EqualTo("AnimTrack"));
            Assert.That(track.type, Is.EqualTo("UnityEngine.Timeline.AnimationTrack"));
            Assert.That(track.bindingInstanceId, Is.EqualTo(goId));
            Assert.That(track.clips, Has.Count.EqualTo(1));
            Assert.That(track.clips[0].displayName, Is.EqualTo("AnimClip"));
            Assert.That(track.clips[0].start, Is.EqualTo(1.0).Within(1e-6));
            Assert.That(track.clips[0].duration, Is.EqualTo(2.0).Within(1e-6));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetTracks_ReadsByAssetPath(CancellationToken cancellationToken)
    {
        // Arrange
        var goId = await CreateAnimationClipSetupAsync(cancellationToken);

        try
        {
            // Act
            var json = await _fixture.TimelineUseCase.GetTracksAsync(0, TimelineAssetPath, cancellationToken);

            // Assert
            var response = JsonSerializer.Deserialize<GetTimelineTracksResponse>(json, s_jsonOptions)!;
            Assert.That(response.tracks, Has.Count.EqualTo(1));
            Assert.That(response.tracks[0].bindingInstanceId, Is.EqualTo(0));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetTrackProperties_ReturnsProperties(CancellationToken cancellationToken)
    {
        // Arrange
        var goId = await CreateAnimationClipSetupAsync(cancellationToken);

        try
        {
            // Act
            var json = await _fixture.TimelineUseCase.GetTrackPropertiesAsync(goId, null, 0, cancellationToken);

            // Assert
            var response = JsonSerializer.Deserialize<GetTimelineTrackPropertiesResponse>(json, s_jsonOptions)!;
            Assert.That(response.typeName, Is.EqualTo("UnityEngine.Timeline.AnimationTrack"));
            Assert.That(response.properties.Exists(p => p.path == "m_TrackOffset"), Is.True);
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetClipProperties_ReadsByAssetPath(CancellationToken cancellationToken)
    {
        // Arrange
        var goId = await CreateAnimationClipSetupAsync(cancellationToken);

        try
        {
            // Act
            var json = await _fixture.TimelineUseCase.GetClipPropertiesAsync(0, TimelineAssetPath, 0, 0,
                cancellationToken);

            // Assert
            var response = JsonSerializer.Deserialize<GetTimelineClipPropertiesResponse>(json, s_jsonOptions)!;
            Assert.That(response.typeName, Is.EqualTo("UnityEngine.Timeline.AnimationPlayableAsset"));
            Assert.That(response.properties.Exists(p => p.path == "m_Clip"), Is.True);
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetClipProperties_Throws_WhenClipIndexOutOfRange(CancellationToken cancellationToken)
    {
        // Arrange
        var goId = await CreateAnimationClipSetupAsync(cancellationToken);

        try
        {
            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.TimelineUseCase.GetClipPropertiesAsync(goId, null, 0, 5, cancellationToken));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask ModifyClip_ChangesOnlyGivenValues(CancellationToken cancellationToken)
    {
        // Arrange
        var goId = await CreateAnimationClipSetupAsync(cancellationToken);
        var request = new ModifyTimelineClipRequest
        {
            instanceId = goId,
            trackIndex = 0,
            clipIndex = 0,
            start = 0.5,
            easeOutDuration = 0.5,
            postExtrapolation = TimelineClipExtrapolationModes.PingPong
        };

        try
        {
            // Act
            var message = await _fixture.TimelineUseCase.ModifyClipAsync(request, cancellationToken);

            // Assert
            Assert.That(message, Does.Contain("Clip updated"));
            var clip = await GetFirstClipAsync(goId, cancellationToken);
            Assert.That(clip.start, Is.EqualTo(0.5).Within(1e-6));
            Assert.That(clip.duration, Is.EqualTo(2.0).Within(1e-6));
            Assert.That(clip.easeOutDuration, Is.EqualTo(0.5).Within(1e-6));
            Assert.That(clip.postExtrapolation, Is.EqualTo(TimelineClipExtrapolationModes.PingPong));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask ModifyClip_Throws_WhenExtrapolationIsInvalid(CancellationToken cancellationToken)
    {
        // Arrange
        var goId = await CreateAnimationClipSetupAsync(cancellationToken);
        var request = new ModifyTimelineClipRequest
            { instanceId = goId, trackIndex = 0, clipIndex = 0, postExtrapolation = "Forever" };

        try
        {
            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _fixture.TimelineUseCase.ModifyClipAsync(request, cancellationToken));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SetClipProperty_AssignsAnimationClip(CancellationToken cancellationToken)
    {
        // Arrange
        var goId = await CreateAnimationClipSetupAsync(cancellationToken);
        await _fixture.AnimationClipUseCase.CreateAsync(AnimationClipPath, false, 60f, cancellationToken);

        try
        {
            // Act
            var message = await _fixture.TimelineUseCase.SetClipPropertyAsync(goId, 0, 0, "m_Clip",
                AnimationClipPath, cancellationToken);

            // Assert
            Assert.That(message, Does.Contain(AnimationClipPath));
            var clip = await GetFirstClipAsync(goId, cancellationToken);
            Assert.That(clip.animationClipPath, Is.EqualTo(AnimationClipPath));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SetClipProperty_SetsValue(CancellationToken cancellationToken)
    {
        // Arrange
        var goId = await CreateAnimationClipSetupAsync(cancellationToken);

        try
        {
            // Act
            var message = await _fixture.TimelineUseCase.SetClipPropertyAsync(goId, 0, 0,
                "m_RemoveStartOffset", "false", cancellationToken);

            // Assert
            Assert.That(message, Does.Contain("m_RemoveStartOffset"));
            var json = await _fixture.TimelineUseCase.GetClipPropertiesAsync(goId, null, 0, 0, cancellationToken);
            var response = JsonSerializer.Deserialize<GetTimelineClipPropertiesResponse>(json, s_jsonOptions)!;
            var property = response.properties.Find(p => p.path == "m_RemoveStartOffset")!;
            Assert.That(property.value, Is.EqualTo("false"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask SetTrackProperty_SetsValue(CancellationToken cancellationToken)
    {
        // Arrange
        var goId = await CreateAnimationClipSetupAsync(cancellationToken);

        try
        {
            // Act
            var message = await _fixture.TimelineUseCase.SetTrackPropertyAsync(goId, 0, "m_Muted", "true",
                cancellationToken);

            // Assert
            Assert.That(message, Does.Contain("m_Muted"));
            var json = await _fixture.TimelineUseCase.GetTracksAsync(goId, null, cancellationToken);
            var response = JsonSerializer.Deserialize<GetTimelineTracksResponse>(json, s_jsonOptions)!;
            Assert.That(response.tracks[0].muted, Is.True);
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, cancellationToken);
        }
    }
}
