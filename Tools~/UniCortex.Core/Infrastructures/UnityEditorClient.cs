using System.Net.Mime;
using System.Text;
using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Infrastructures;

public class UnityEditorClient(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
    : IUnityEditorClient
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);
    private string BaseUrl => urlProvider.GetUrl();

    /// <summary>
    /// POST with optional JSON body, return the response deserialized as <typeparamref name="TRes"/>.
    /// </summary>
    public async ValueTask<TRes> PostAsync<TReq, TRes>(string route, TReq? request = null,
        CancellationToken cancellationToken = default) where TReq : class
    {
        var baseUrl = BaseUrl;
        await WaitForServerAsync(cancellationToken);

        HttpContent? content = request != null
            ? new StringContent(
                JsonSerializer.Serialize(request, JsonOptions.Default),
                Encoding.UTF8,
                MediaTypeNames.Application.Json)
            : null;
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}{route}") { Content = content };
        using var response = await SendAsync(httpRequest, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<TRes>(json, JsonOptions.Default)!;
    }

    /// <summary>
    /// GET with optional query parameters, return the response deserialized as <typeparamref name="TRes"/>.
    /// </summary>
    public async ValueTask<TRes> GetAsync<TReq, TRes>(string route, TReq? request = null,
        CancellationToken cancellationToken = default) where TReq : class
    {
        var baseUrl = BaseUrl;
        await WaitForServerAsync(cancellationToken);

        var queryString = request != null ? BuildQueryString(request) : "";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{route}{queryString}");
        using var response = await SendAsync(httpRequest, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<TRes>(json, JsonOptions.Default)!;
    }

    /// <summary>
    /// Poll GET /ping until the server responds with a non-empty body.
    /// HttpRequestHandler handles retries for GET requests during domain reload.
    /// </summary>
    public async ValueTask WaitForServerAsync(CancellationToken cancellationToken = default)
    {
        using var pingRequest = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}{ApiRoutes.Ping}");
        using var pingResponse = await SendAsync(pingRequest, cancellationToken);
        await pingResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            // HttpClient reports its timeout as a cancellation. The dialogs are read while the request waits
            // (EditorDialogWatchHandler), so a request blocked by a dialog that can be read has failed before this.
            throw new TimeoutException(
                $"The Unity Editor did not respond within {_httpClient.Timeout.TotalMinutes:0.#} minutes. It may be " +
                "blocked by a modal dialog that UniCortex cannot read (e.g. without the Accessibility permission on " +
                "macOS), or still be running a long operation. Call get_editor_dialogs (CLI: editor dialog list), " +
                "which tells why the dialogs cannot be read, or ask the user to check the Editor. The request was " +
                "already sent to the Editor and may still run, so check the result before retrying the request.",
                ex);
        }
    }

    private static string BuildQueryString<T>(T request)
    {
        var element = JsonSerializer.SerializeToElement(request, JsonOptions.Default);
        var pairs = new List<string>();
        foreach (var prop in element.EnumerateObject())
        {
            if (prop.Value.ValueKind == JsonValueKind.Null)
                continue;
            var value = prop.Value.ValueKind switch
            {
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => prop.Value.ToString()
            };
            pairs.Add($"{Uri.EscapeDataString(prop.Name)}={Uri.EscapeDataString(value)}");
        }

        return pairs.Count > 0 ? $"?{string.Join("&", pairs)}" : "";
    }
}
