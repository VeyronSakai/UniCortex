using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class TimelineUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> CreateAsync(string assetPath, CancellationToken cancellationToken)
    {
        var request = new CreateTimelineRequest { assetPath = assetPath };
        var response = await client.PostAsync<CreateTimelineRequest, CreateTimelineResponse>(
            ApiRoutes.TimelineCreate, request, cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> AddTrackAsync(int instanceId, string trackType, string trackName,
        CancellationToken cancellationToken)
    {
        var request = new AddTimelineTrackRequest
            { instanceId = instanceId, trackType = trackType, trackName = trackName };
        await client.PostAsync<AddTimelineTrackRequest, AddTimelineTrackResponse>(ApiRoutes.TimelineAddTrack, request,
            cancellationToken);
        return $"Track added: {trackType} ({trackName})";
    }

    public async ValueTask<string> RemoveTrackAsync(int instanceId, int trackIndex,
        CancellationToken cancellationToken)
    {
        var request = new RemoveTimelineTrackRequest { instanceId = instanceId, trackIndex = trackIndex };
        await client.PostAsync<RemoveTimelineTrackRequest, RemoveTimelineTrackResponse>(ApiRoutes.TimelineRemoveTrack, request,
            cancellationToken);
        return $"Track removed at index {trackIndex}";
    }

    public async ValueTask<string> BindTrackAsync(int instanceId, int trackIndex, int targetInstanceId,
        CancellationToken cancellationToken)
    {
        var request = new BindTimelineTrackRequest
            { instanceId = instanceId, trackIndex = trackIndex, targetInstanceId = targetInstanceId };
        await client.PostAsync<BindTimelineTrackRequest, BindTimelineTrackResponse>(ApiRoutes.TimelineBindTrack, request,
            cancellationToken);
        return $"Track binded: track {trackIndex} -> instanceId {targetInstanceId}";
    }

    public async ValueTask<string> AddClipAsync(int instanceId, int trackIndex, double start, double duration,
        string clipName, CancellationToken cancellationToken)
    {
        var request = new AddTimelineClipRequest
            { instanceId = instanceId, trackIndex = trackIndex, start = start, duration = duration, clipName = clipName };
        await client.PostAsync<AddTimelineClipRequest, AddTimelineClipResponse>(ApiRoutes.TimelineAddClip, request,
            cancellationToken);
        return $"Clip added to track {trackIndex} at {start}s";
    }

    public async ValueTask<string> RemoveClipAsync(int instanceId, int trackIndex, int clipIndex,
        CancellationToken cancellationToken)
    {
        var request = new RemoveTimelineClipRequest
            { instanceId = instanceId, trackIndex = trackIndex, clipIndex = clipIndex };
        await client.PostAsync<RemoveTimelineClipRequest, RemoveTimelineClipResponse>(ApiRoutes.TimelineRemoveClip, request,
            cancellationToken);
        return $"Clip removed: track {trackIndex}, clip {clipIndex}";
    }

    public async ValueTask<string> PlayAsync(int instanceId, CancellationToken cancellationToken)
    {
        var request = new PlayTimelineRequest { instanceId = instanceId };
        await client.PostAsync<PlayTimelineRequest, PlayTimelineResponse>(
            ApiRoutes.TimelinePlay, request, cancellationToken);
        return "Timeline playback started";
    }

    public async ValueTask<string> StopAsync(int instanceId, CancellationToken cancellationToken)
    {
        var request = new StopTimelineRequest { instanceId = instanceId };
        await client.PostAsync<StopTimelineRequest, StopTimelineResponse>(
            ApiRoutes.TimelineStop, request, cancellationToken);
        return "Timeline playback stopped";
    }
}
