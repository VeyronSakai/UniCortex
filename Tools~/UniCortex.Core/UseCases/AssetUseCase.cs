using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class AssetUseCase(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    public async ValueTask<string> RefreshAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.AssetDatabaseRefresh}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return "Asset database refreshed.";
    }
}
