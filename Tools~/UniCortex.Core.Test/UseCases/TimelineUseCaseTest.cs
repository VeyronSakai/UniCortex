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

    private async ValueTask<(int goInstanceId, int directorInstanceId)> CreateTimelineSetupAsync()
    {
        var ct = CancellationToken.None;
        var goJson = await _fixture.GameObjectUseCase.CreateAsync("TimelineTestObj", ct);
        var goResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(goJson, s_jsonOptions)!;

        var createJson = await _fixture.TimelineUseCase.CreateAsync(goResponse.instanceId, TimelineAssetPath, ct);
        var createResponse = JsonSerializer.Deserialize<CreateTimelineResponse>(createJson, s_jsonOptions)!;

        return (goResponse.instanceId, createResponse.instanceId);
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask Create_CreatesTimelineAssetAndAssignsToDirector()
    {
        var ct = CancellationToken.None;
        var goJson = await _fixture.GameObjectUseCase.CreateAsync("CreateTimelineTestObj", ct);
        var goResponse = JsonSerializer.Deserialize<CreateGameObjectResponse>(goJson, s_jsonOptions)!;

        try
        {
            var json = await _fixture.TimelineUseCase.CreateAsync(goResponse.instanceId, TimelineAssetPath, ct);
            var response = JsonSerializer.Deserialize<CreateTimelineResponse>(json, s_jsonOptions)!;

            Assert.That(response.assetPath, Is.EqualTo(TimelineAssetPath));
            Assert.That(response.instanceId, Is.Not.EqualTo(0));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goResponse.instanceId, ct);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetInfo_ReturnsTimelineInfo_WhenTimelineExists()
    {
        var ct = CancellationToken.None;
        var (goId, _) = await CreateTimelineSetupAsync();

        try
        {
            var json = await _fixture.TimelineUseCase.GetInfoAsync(goId, ct);

            Assert.That(json, Does.Contain("timelineAssetName"));
            Assert.That(json, Does.Contain("tracks"));
            Assert.That(json, Does.Contain("bindings"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, ct);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask AddTrack_AddsTrackToTimeline()
    {
        var ct = CancellationToken.None;
        var (goId, _) = await CreateTimelineSetupAsync();

        try
        {
            var message = await _fixture.TimelineUseCase.AddTrackAsync(
                goId, TimelineTrackType.ActivationTrack, "TestTrack", ct);

            Assert.That(message, Does.Contain("Track added"));

            var infoJson = await _fixture.TimelineUseCase.GetInfoAsync(goId, ct);
            Assert.That(infoJson, Does.Contain("TestTrack"));
            Assert.That(infoJson, Does.Contain("ActivationTrack"));
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
        var (goId, _) = await CreateTimelineSetupAsync();

        try
        {
            await _fixture.TimelineUseCase.AddTrackAsync(
                goId, TimelineTrackType.ActivationTrack, "TrackToRemove", ct);

            var message = await _fixture.TimelineUseCase.RemoveTrackAsync(goId, 0, ct);

            Assert.That(message, Does.Contain("Track removed"));

            var infoJson = await _fixture.TimelineUseCase.GetInfoAsync(goId, ct);
            Assert.That(infoJson, Does.Not.Contain("TrackToRemove"));
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
        var (goId, _) = await CreateTimelineSetupAsync();

        try
        {
            await _fixture.TimelineUseCase.AddTrackAsync(
                goId, TimelineTrackType.ActivationTrack, "ClipTestTrack", ct);

            var message = await _fixture.TimelineUseCase.AddClipAsync(goId, 0, 1.0, 3.0, "TestClip", ct);

            Assert.That(message, Does.Contain("Clip added"));

            var infoJson = await _fixture.TimelineUseCase.GetInfoAsync(goId, ct);
            Assert.That(infoJson, Does.Contain("TestClip"));
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
        var (goId, _) = await CreateTimelineSetupAsync();

        try
        {
            await _fixture.TimelineUseCase.AddTrackAsync(
                goId, TimelineTrackType.ActivationTrack, "ClipRemoveTrack", ct);
            await _fixture.TimelineUseCase.AddClipAsync(goId, 0, 0, 5.0, "ClipToRemove", ct);

            var message = await _fixture.TimelineUseCase.RemoveClipAsync(goId, 0, 0, ct);

            Assert.That(message, Does.Contain("Clip removed"));

            var infoJson = await _fixture.TimelineUseCase.GetInfoAsync(goId, ct);
            Assert.That(infoJson, Does.Not.Contain("ClipToRemove"));
        }
        finally
        {
            await _fixture.GameObjectUseCase.DeleteAsync(goId, ct);
        }
    }

    [Test, CancelAfter(120_000)]
    public async ValueTask GetInfo_ReturnsError_WhenInvalidInstanceId()
    {
        var ex = Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _fixture.TimelineUseCase.GetInfoAsync(-1, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("instanceId").Or.Contain("Timeline"));
    }
}
