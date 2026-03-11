using System.Text;
using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class SceneUseCase(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    public async ValueTask<string> CreateAsync(string scenePath, CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var body = JsonSerializer.Serialize(new CreateSceneRequest { scenePath = scenePath },
            new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.SceneCreate}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Scene created: {scenePath}";
    }

    public async ValueTask<string> OpenAsync(string scenePath, CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var body = JsonSerializer.Serialize(new OpenSceneRequest { scenePath = scenePath },
            new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.SceneOpen}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Scene opened: {scenePath}";
    }

    public async ValueTask<string> SaveAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.SceneSave}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return "Scene saved successfully.";
    }

    public async ValueTask<string> GetHierarchyAsync(CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        using var response = await _httpClient.GetAsync($"{baseUrl}{ApiRoutes.SceneHierarchy}", cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
