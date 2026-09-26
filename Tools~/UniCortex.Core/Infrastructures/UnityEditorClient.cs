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
        using var response = await _httpClient.PostAsync($"{baseUrl}{route}", content, cancellationToken);
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
        using var response = await _httpClient.GetAsync($"{baseUrl}{route}{queryString}", cancellationToken);
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
        using var pingResponse = await _httpClient.GetAsync($"{BaseUrl}{ApiRoutes.Ping}", cancellationToken);
        await pingResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);
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
