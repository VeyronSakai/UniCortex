using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class EditorUseCase(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private static readonly TimeSpan s_pollInterval = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan s_pollTimeout = TimeSpan.FromSeconds(30);

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    public async ValueTask<string> PingAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();

        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        using var response = await _httpClient.GetAsync(
            $"{baseUrl}{ApiRoutes.Ping}?{QueryParameterNames.Verbose}=true", cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var ping = JsonSerializer.Deserialize<PingResponse>(json, s_jsonOptions)!;
        return ping.message;
    }

    public async ValueTask<string> EnterPlayModeAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        if (await GetIsPlayingAsync(baseUrl, cancellationToken))
        {
            return "Editor is already in play mode.";
        }

        using var response = await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.Play}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        await WaitForPlayModeStateAsync(baseUrl, expectedPlaying: true, cancellationToken);
        return "Play mode started successfully.";
    }

    public async ValueTask<string> ExitPlayModeAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();

        if (!await GetIsPlayingAsync(baseUrl, cancellationToken))
        {
            return "Editor is not in play mode.";
        }

        using var response = await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.Stop}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        await WaitForPlayModeStateAsync(baseUrl, expectedPlaying: false, cancellationToken);
        return "Play mode stopped successfully.";
    }

    public async ValueTask<string> UndoAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        using var response = await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.Undo}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return "Undo performed successfully.";
    }

    public async ValueTask<string> RedoAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        using var response = await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.Redo}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return "Redo performed successfully.";
    }

    public async ValueTask<string> ReloadDomainAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        using var response = await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.DomainReload}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        // Poll /ping to wait for the server to come back after domain reload.
        // HttpRequestHandler handles retries during the reload.
        using var pingResponse = await _httpClient.GetAsync($"{baseUrl}{ApiRoutes.Ping}", cancellationToken);
        await pingResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        return "Domain reload completed successfully.";
    }

    private async ValueTask<bool> GetIsPlayingAsync(string baseUrl, CancellationToken cancellationToken)
    {
        using var statusResponse = await _httpClient.GetAsync($"{baseUrl}{ApiRoutes.Status}", cancellationToken);
        await statusResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        var statusJson = await statusResponse.Content.ReadAsStringAsync(cancellationToken);
        var status = JsonSerializer.Deserialize<EditorStatusResponse>(statusJson, s_jsonOptions)!;
        return status.isPlaying;
    }

    private async ValueTask WaitForPlayModeStateAsync(string baseUrl, bool expectedPlaying,
        CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + s_pollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(s_pollInterval, cancellationToken);
            if (await GetIsPlayingAsync(baseUrl, cancellationToken) == expectedPlaying)
            {
                return;
            }
        }

        throw new TimeoutException(
            $"Timed out waiting for Editor to {(expectedPlaying ? "enter" : "exit")} play mode.");
    }
}
