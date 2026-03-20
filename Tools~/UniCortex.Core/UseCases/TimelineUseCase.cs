using UniCortex.Core.Infrastructures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class TimelineUseCase(UnityEditorClient client)
{
    public ValueTask<string> CreateAsync(string assetPath, CancellationToken cancellationToken)
    {
        var request = new CreateTimelineRequest { assetPath = assetPath };
        return client.PostAndReadAsync(ApiRoutes.TimelineCreate, request, cancellationToken);
    }

    public ValueTask<string> AddTrackAsync(int instanceId, string trackType, string trackName,
        CancellationToken cancellationToken)
    {
        var request = new AddTimelineTrackRequest
            { instanceId = instanceId, trackType = trackType, trackName = trackName };
        return client.PostAsync(ApiRoutes.TimelineAddTrack, request,
            $"Track added: {trackType} ({trackName})", cancellationToken);
    }

    public ValueTask<string> RemoveTrackAsync(int instanceId, int trackIndex,
        CancellationToken cancellationToken)
    {
        var request = new RemoveTimelineTrackRequest { instanceId = instanceId, trackIndex = trackIndex };
        return client.PostAsync(ApiRoutes.TimelineRemoveTrack, request,
            $"Track removed at index {trackIndex}", cancellationToken);
    }

    public ValueTask<string> BindTrackAsync(int instanceId, int trackIndex, int targetInstanceId,
        CancellationToken cancellationToken)
    {
        var request = new BindTimelineTrackRequest
            { instanceId = instanceId, trackIndex = trackIndex, targetInstanceId = targetInstanceId };
        return client.PostAsync(ApiRoutes.TimelineBindTrack, request,
            $"Track binded: track {trackIndex} -> instanceId {targetInstanceId}", cancellationToken);
    }

    public ValueTask<string> AddClipAsync(int instanceId, int trackIndex, double start, double duration,
        string clipName, CancellationToken cancellationToken)
    {
        var request = new AddTimelineClipRequest
            { instanceId = instanceId, trackIndex = trackIndex, start = start, duration = duration, clipName = clipName };
        return client.PostAsync(ApiRoutes.TimelineAddClip, request,
            $"Clip added to track {trackIndex} at {start}s", cancellationToken);
    }

    public ValueTask<string> RemoveClipAsync(int instanceId, int trackIndex, int clipIndex,
        CancellationToken cancellationToken)
    {
        var request = new RemoveTimelineClipRequest
            { instanceId = instanceId, trackIndex = trackIndex, clipIndex = clipIndex };
        return client.PostAsync(ApiRoutes.TimelineRemoveClip, request,
            $"Clip removed: track {trackIndex}, clip {clipIndex}", cancellationToken);
    }
}
