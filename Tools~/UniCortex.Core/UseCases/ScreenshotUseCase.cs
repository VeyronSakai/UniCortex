using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ScreenshotUseCase(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    public async ValueTask<byte[]> CaptureAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

        var url = $"{baseUrl}{ApiRoutes.ScreenshotCapture}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
