using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class EditorUseCase(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private static readonly TimeSpan s_pollInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan s_pollTimeout = TimeSpan.FromMinutes(10);

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    public async ValueTask<string> PingAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();

        await WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

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
        await WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

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

        // Wait for the server to become available before triggering domain reload.
        // If Unity is already auto-recompiling (e.g. after a .cs file change),
        // the server will be unavailable; this prevents a double RequestScriptCompilation() call
        // that can freeze Unity.
        await WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        using var response = await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.DomainReload}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        // RequestScriptCompilation() is dispatched asynchronously on the Unity main thread.
        // Wait briefly so that compilation starts and the server becomes unavailable
        // before we begin polling /ping.
        await Task.Delay(100, cancellationToken);

        await WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        return "Domain reload completed successfully.";
    }

    /// <summary>
    /// Poll GET /ping until the server responds with a non-empty body.
    /// HttpRequestHandler handles retries for GET requests during domain reload.
    /// </summary>
    public static async ValueTask WaitForServerAsync(HttpClient httpClient, string baseUrl,
        CancellationToken cancellationToken)
    {
        using var pingResponse = await httpClient.GetAsync($"{baseUrl}{ApiRoutes.Ping}", cancellationToken);
        await pingResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);
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
