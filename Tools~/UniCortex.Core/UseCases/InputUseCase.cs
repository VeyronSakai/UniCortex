using System.Text;
using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class InputUseCase(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    public async ValueTask<string> SendKeyEventAsync(string key, string eventType,
        CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new SendKeyEventRequest { key = key, eventType = eventType };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.InputKey}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Key event sent: {key} ({eventType})";
    }

    public async ValueTask<string> SendMouseEventAsync(float x, float y, string button, string eventType,
        CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new SendMouseEventRequest { x = x, y = y, button = button, eventType = eventType };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.InputMouse}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Mouse event sent: ({x}, {y}) button={button} ({eventType})";
    }
}
