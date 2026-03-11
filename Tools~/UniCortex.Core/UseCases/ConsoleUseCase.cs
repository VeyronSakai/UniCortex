using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ConsoleUseCase(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    public async ValueTask<string> GetLogsAsync(int? count = null, bool? stackTrace = null,
        bool? log = null, bool? warning = null, bool? error = null,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var queryParams = new List<string>();
        if (count.HasValue) queryParams.Add($"count={count.Value}");
        if (stackTrace.HasValue) queryParams.Add($"stackTrace={stackTrace.Value.ToString().ToLowerInvariant()}");
        if (log.HasValue) queryParams.Add($"log={log.Value.ToString().ToLowerInvariant()}");
        if (warning.HasValue) queryParams.Add($"warning={warning.Value.ToString().ToLowerInvariant()}");
        if (error.HasValue) queryParams.Add($"error={error.Value.ToString().ToLowerInvariant()}");

        var url = $"{baseUrl}{ApiRoutes.ConsoleLogs}";
        if (queryParams.Count > 0)
        {
            url = $"{url}?{string.Join("&", queryParams)}";
        }

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async ValueTask<string> ClearAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.ConsoleClear}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return "Console logs cleared successfully.";
    }
}
