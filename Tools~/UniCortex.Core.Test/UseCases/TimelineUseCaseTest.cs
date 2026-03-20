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
        await _fixture.SceneUseCase.SaveAsync(CancellationToken.None);
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
    public async ValueTask Create_CreatesTimelineAsset()
    {
        var ct = CancellationToken.None;

        var json = await _fixture.TimelineUseCase.CreateAsync(TimelineAssetPath, ct);

        Assert.That(json, Does.Contain(TimelineAssetPath));
        Assert.That(json, Does.Contain("true"));
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask AddTrack_AddsTrackToTimeline()
    {
        var ct = CancellationToken.None;
        var goId = await CreateTimelineSetupAsync(ct);

        try
        {
            var message = await _fixture.TimelineUseCase.AddTrackAsync(
                goId, "UnityEngine.Timeline.ActivationTrack", "TestTrack", ct);

            Assert.That(message, Does.Contain("Track added"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, ct);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask RemoveTrack_RemovesTrackFromTimeline()
    {
        var ct = CancellationToken.None;
        var goId = await CreateTimelineSetupAsync(ct);

        try
        {
            await _fixture.TimelineUseCase.AddTrackAsync(
                goId, "UnityEngine.Timeline.ActivationTrack", "TrackToRemove", ct);

            var message = await _fixture.TimelineUseCase.RemoveTrackAsync(goId, 0, ct);

            Assert.That(message, Does.Contain("Track removed"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, ct);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask AddClip_AddsClipToTrack()
    {
        var ct = CancellationToken.None;
        var goId = await CreateTimelineSetupAsync(ct);

        try
        {
            await _fixture.TimelineUseCase.AddTrackAsync(
                goId, "UnityEngine.Timeline.ActivationTrack", "ClipTestTrack", ct);

            var message = await _fixture.TimelineUseCase.AddClipAsync(goId, 0, 1.0, 3.0, "TestClip", ct);

            Assert.That(message, Does.Contain("Clip added"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, ct);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask RemoveClip_RemovesClipFromTrack()
    {
        var ct = CancellationToken.None;
        var goId = await CreateTimelineSetupAsync(ct);

        try
        {
            await _fixture.TimelineUseCase.AddTrackAsync(
                goId, "UnityEngine.Timeline.ActivationTrack", "ClipRemoveTrack", ct);
            await _fixture.TimelineUseCase.AddClipAsync(goId, 0, 0, 5.0, "ClipToRemove", ct);

            var message = await _fixture.TimelineUseCase.RemoveClipAsync(goId, 0, 0, ct);

            Assert.That(message, Does.Contain("Clip removed"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, ct);
        }
    }

}
