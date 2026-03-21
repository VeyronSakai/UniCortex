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
    /// POST with JSON body, return the response body as string.
    /// </summary>
    public async ValueTask<string> PostAsync<T>(string route, T request,
        CancellationToken cancellationToken)
    {
        var baseUrl = BaseUrl;
        await WaitForServerAsync(cancellationToken);

        var body = JsonSerializer.Serialize(request, JsonOptions.Default);
        var content = new StringContent(body, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
        using var response = await _httpClient.PostAsync($"{baseUrl}{route}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    /// POST with null body.
    /// </summary>
    public async ValueTask PostAsync(string route, CancellationToken cancellationToken)
    {
        var baseUrl = BaseUrl;
        await WaitForServerAsync(cancellationToken);

        using var response = await _httpClient.PostAsync($"{baseUrl}{route}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
    }

    /// <summary>
    /// GET and return the response body as string.
    /// </summary>
    public async ValueTask<string> GetStringAsync(string route, CancellationToken cancellationToken)
    {
        var baseUrl = BaseUrl;
        await WaitForServerAsync(cancellationToken);

        using var response = await _httpClient.GetAsync($"{baseUrl}{route}", cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    /// GET and return the response body as byte[].
    /// </summary>
    public async ValueTask<byte[]> GetBytesAsync(string route, CancellationToken cancellationToken)
    {
        var baseUrl = BaseUrl;
        await WaitForServerAsync(cancellationToken);

        using var response = await _httpClient.GetAsync($"{baseUrl}{route}", cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Poll GET /ping until the server responds with a non-empty body.
    /// HttpRequestHandler handles retries for GET requests during domain reload.
    /// </summary>
    public async ValueTask WaitForServerAsync(CancellationToken cancellationToken)
    {
        using var pingResponse = await _httpClient.GetAsync($"{BaseUrl}{ApiRoutes.Ping}", cancellationToken);
        await pingResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);
    }

    /// <summary>
    /// Low-level GET without WaitForServer or EnsureSuccess. Caller owns the response.
    /// </summary>
    public async ValueTask<HttpResponseMessage> SendGetAsync(string route, CancellationToken cancellationToken)
    {
        return await _httpClient.GetAsync($"{BaseUrl}{route}", cancellationToken);
    }

    /// <summary>
    /// Low-level POST without WaitForServer or EnsureSuccess. Caller owns the response.
    /// </summary>
    public async ValueTask<HttpResponseMessage> SendPostAsync(string route, HttpContent? content,
        CancellationToken cancellationToken)
    {
        return await _httpClient.PostAsync($"{BaseUrl}{route}", content, cancellationToken);
    }
}
