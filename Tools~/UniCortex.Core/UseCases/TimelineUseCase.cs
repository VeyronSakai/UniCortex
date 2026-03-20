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

    public async ValueTask<string> GetInfoAsync(int instanceId, CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        using var response = await _httpClient.GetAsync(
            $"{baseUrl}{ApiRoutes.TimelineInfo}?instanceId={instanceId}", cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async ValueTask<string> SetTimeAsync(int instanceId, double time,
        CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new SetTimelineTimeRequest { instanceId = instanceId, time = time };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.TimelineSetTime}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Timeline time set to {time}";
    }

    public async ValueTask<string> PlayAsync(int instanceId, CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new PlayTimelineRequest { instanceId = instanceId };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.TimelinePlay}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return "Timeline playing";
    }

    public async ValueTask<string> PauseAsync(int instanceId, CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new PauseTimelineRequest { instanceId = instanceId };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.TimelinePause}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return "Timeline paused";
    }

    public async ValueTask<string> StopAsync(int instanceId, CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new StopTimelineRequest { instanceId = instanceId };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.TimelineStop}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return "Timeline stopped";
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
}
