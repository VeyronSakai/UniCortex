using System.Text;
using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Services;

public class PrefabService(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    public async ValueTask<string> CreateAsync(int instanceId, string assetPath,
        CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

        var request = new CreatePrefabRequest { instanceId = instanceId, assetPath = assetPath };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.PrefabCreate}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Prefab created at: {assetPath}";
    }

    public async ValueTask<string> InstantiateAsync(string assetPath, CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

        var request = new InstantiatePrefabRequest { assetPath = assetPath };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.PrefabInstantiate}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
