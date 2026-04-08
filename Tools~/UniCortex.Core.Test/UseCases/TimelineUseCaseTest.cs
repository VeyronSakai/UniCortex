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
        var goJson = await _fixture.GameObjectUseCase.CreateAsync("TimelineTestObj", cancellationToken);
        var goResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(goJson, s_jsonOptions)!;

        // Add PlayableDirector component
        await _fixture.ComponentUseCase.AddAsync(goResponse.instanceId,
            "UnityEngine.Playables.PlayableDirector", cancellationToken);

        // Assign the TimelineAsset to PlayableDirector via set_component_property
        await _fixture.ComponentUseCase.SetPropertyAsync(goResponse.instanceId,
            "UnityEngine.Playables.PlayableDirector", "m_PlayableAsset", TimelineAssetPath, cancellationToken);

        return goResponse.instanceId;
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
}
