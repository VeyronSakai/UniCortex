using System.Text;
using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class TimelineUseCase(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    public async ValueTask<string> CreateAsync(int instanceId, string assetPath,
        CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new CreateTimelineRequest { instanceId = instanceId, assetPath = assetPath };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.TimelineCreate}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async ValueTask<string> GetInfoAsync(int instanceId, CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        using var response = await _httpClient.GetAsync(
            $"{baseUrl}{ApiRoutes.TimelineInfo}?instanceId={instanceId}", cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async ValueTask<string> AddTrackAsync(int instanceId, string trackType, string trackName,
        CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new AddTimelineTrackRequest
            { instanceId = instanceId, trackType = trackType, trackName = trackName };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.TimelineAddTrack}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Track added: {trackType} ({trackName})";
    }

    public async ValueTask<string> RemoveTrackAsync(int instanceId, int trackIndex,
        CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new RemoveTimelineTrackRequest { instanceId = instanceId, trackIndex = trackIndex };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.TimelineRemoveTrack}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Track removed at index {trackIndex}";
    }

    public async ValueTask<string> SetBindingAsync(int instanceId, int trackIndex, int targetInstanceId,
        CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new SetTimelineBindingRequest
            { instanceId = instanceId, trackIndex = trackIndex, targetInstanceId = targetInstanceId };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.TimelineSetBinding}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Binding set: track {trackIndex} -> instanceId {targetInstanceId}";
    }

    public async ValueTask<string> AddClipAsync(int instanceId, int trackIndex, double start, double duration,
        string clipName, CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new AddTimelineClipRequest
            { instanceId = instanceId, trackIndex = trackIndex, start = start, duration = duration, clipName = clipName };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.TimelineAddClip}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Clip added to track {trackIndex} at {start}s";
    }

    public async ValueTask<string> RemoveClipAsync(int instanceId, int trackIndex, int clipIndex,
        CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new RemoveTimelineClipRequest
            { instanceId = instanceId, trackIndex = trackIndex, clipIndex = clipIndex };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.TimelineRemoveClip}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Clip removed: track {trackIndex}, clip {clipIndex}";
    }
}
