using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Services;

public class EditorService(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    public async ValueTask<string> PingAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();

        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var response = await _httpClient.GetAsync(
            $"{baseUrl}{ApiRoutes.Ping}?{QueryParameterNames.Verbose}=true", cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var ping = JsonSerializer.Deserialize<PingResponse>(json,
            new JsonSerializerOptions { IncludeFields = true })!;
        return ping.message;
    }

    public async ValueTask<string> EnterPlayModeAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

        var response = await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.Play}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        while (true)
        {
            var statusResponse = await _httpClient.GetAsync($"{baseUrl}{ApiRoutes.Status}", cancellationToken);
            await statusResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);
            var statusJson = await statusResponse.Content.ReadAsStringAsync(cancellationToken);
            var status = JsonSerializer.Deserialize<EditorStatusResponse>(statusJson,
                new JsonSerializerOptions { IncludeFields = true })!;
            if (status.isPlaying)
            {
                return "Play mode started successfully.";
            }
        }
    }

    public async ValueTask<string> ExitPlayModeAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        var response = await _httpClient.PostAsync(baseUrl + ApiRoutes.Stop, null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        while (true)
        {
            var statusResponse = await _httpClient.GetAsync(baseUrl + ApiRoutes.Status, cancellationToken);
            await statusResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);
            var statusJson = await statusResponse.Content.ReadAsStringAsync(cancellationToken);
            var status = JsonSerializer.Deserialize<EditorStatusResponse>(statusJson,
                new JsonSerializerOptions { IncludeFields = true })!;
            if (!status.isPlaying)
            {
                return "Play mode stopped successfully.";
            }
        }
    }

    public async ValueTask<string> UndoAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        var response = await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.Undo}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return "Undo performed successfully.";
    }

    public async ValueTask<string> RedoAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        var response = await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.Redo}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return "Redo performed successfully.";
    }

    public async ValueTask<string> ReloadDomainAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        var response = await _httpClient.PostAsync(baseUrl + ApiRoutes.DomainReload, null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        // Poll /ping to wait for the server to come back after domain reload.
        // HttpRequestHandler handles retries during the reload.
        var pingResponse = await _httpClient.GetAsync($"{baseUrl}{ApiRoutes.Ping}", cancellationToken);
        await pingResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        return "Domain reload completed successfully.";
    }
}
