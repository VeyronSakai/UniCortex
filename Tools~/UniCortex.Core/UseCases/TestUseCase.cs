using System.Text;
using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class TestUseCase(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    public async ValueTask<string> RunAsync(string? testMode = null, string[]? testNames = null,
        string[]? groupNames = null, string[]? categoryNames = null, string[]? assemblyNames = null,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new RunTestsRequest(
            testMode ?? TestModes.EditMode,
            testNames != null ? new List<string>(testNames) : null,
            groupNames != null ? new List<string>(groupNames) : null,
            categoryNames != null ? new List<string>(categoryNames) : null,
            assemblyNames != null ? new List<string>(assemblyNames) : null);
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        string responseJson;
        try
        {
            using var response = await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.TestsRun}", content,
                cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
            responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            // Server disrupted (e.g., domain reload during PlayMode entry)
            responseJson = "";
        }

        // PlayMode tests trigger a domain reload when entering play mode,
        // which disrupts the HTTP server. Poll GET /tests/result for results.
        // The HttpRequestHandler retries GET on Content-Length: 0, so this
        // automatically polls until results are stored in SessionState.
        if (string.IsNullOrEmpty(responseJson))
        {
            await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);
            using var resultResponse =
                await _httpClient.GetAsync($"{baseUrl}{ApiRoutes.TestsResult}", cancellationToken);
            await resultResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);
            responseJson = await resultResponse.Content.ReadAsStringAsync(cancellationToken);
        }

        return responseJson;
    }
}
