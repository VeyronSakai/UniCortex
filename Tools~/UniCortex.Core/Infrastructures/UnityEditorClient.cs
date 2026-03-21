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
    public HttpClient HttpClient { get; } = httpClientFactory.CreateClient(HttpClientNames.UniCortex);
    public string BaseUrl => urlProvider.GetUrl();

    /// <summary>
    /// POST with JSON body, return a fixed success message.
    /// </summary>
    public async ValueTask<string> PostAsync<T>(string route, T request, string successMessage,
        CancellationToken cancellationToken)
    {
        var baseUrl = BaseUrl;
        await WaitForServerAsync(cancellationToken);

        var body = JsonSerializer.Serialize(request, JsonOptions.Default);
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response = await HttpClient.PostAsync($"{baseUrl}{route}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return successMessage;
    }

    /// <summary>
    /// POST with JSON body, return the response body as string.
    /// </summary>
    public async ValueTask<string> PostAndReadAsync<T>(string route, T request,
        CancellationToken cancellationToken)
    {
        var baseUrl = BaseUrl;
        await WaitForServerAsync(cancellationToken);

        var body = JsonSerializer.Serialize(request, JsonOptions.Default);
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response = await HttpClient.PostAsync($"{baseUrl}{route}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    /// POST with null body, return a fixed success message.
    /// </summary>
    public async ValueTask<string> PostEmptyAsync(string route, string successMessage,
        CancellationToken cancellationToken)
    {
        var baseUrl = BaseUrl;
        await WaitForServerAsync(cancellationToken);

        using var response = await HttpClient.PostAsync($"{baseUrl}{route}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return successMessage;
    }

    /// <summary>
    /// GET and return the response body as string.
    /// </summary>
    public async ValueTask<string> GetStringAsync(string route, CancellationToken cancellationToken)
    {
        var baseUrl = BaseUrl;
        await WaitForServerAsync(cancellationToken);

        using var response = await HttpClient.GetAsync($"{baseUrl}{route}", cancellationToken);
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

        using var response = await HttpClient.GetAsync($"{baseUrl}{route}", cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Poll GET /ping until the server responds with a non-empty body.
    /// HttpRequestHandler handles retries for GET requests during domain reload.
    /// </summary>
    public async ValueTask WaitForServerAsync(CancellationToken cancellationToken)
    {
        using var pingResponse = await HttpClient.GetAsync($"{BaseUrl}{ApiRoutes.Ping}", cancellationToken);
        await pingResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);
    }
}
