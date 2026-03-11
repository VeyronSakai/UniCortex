using System.Text;
using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ComponentUseCase(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    public async ValueTask<string> AddAsync(int instanceId, string componentType,
        CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new AddComponentRequest { instanceId = instanceId, componentType = componentType };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.ComponentAdd}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Component '{componentType}' added successfully.";
    }

    public async ValueTask<string> RemoveAsync(int instanceId, string componentType,
        int componentIndex = 0, CancellationToken cancellationToken = default)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new RemoveComponentRequest
        {
            instanceId = instanceId, componentType = componentType, componentIndex = componentIndex
        };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.ComponentRemove}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Component '{componentType}' removed successfully.";
    }

    public async ValueTask<string> GetPropertiesAsync(int instanceId, string componentType,
        int componentIndex = 0, CancellationToken cancellationToken = default)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var url =
            $"{baseUrl}{ApiRoutes.ComponentProperties}?instanceId={instanceId}&componentType={Uri.EscapeDataString(componentType)}&componentIndex={componentIndex}";
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async ValueTask<string> SetPropertyAsync(int instanceId, string componentType,
        string propertyPath, string value, CancellationToken cancellationToken = default)
    {
        var baseUrl = urlProvider.GetUrl();
        await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new SetComponentPropertyRequest
        {
            instanceId = instanceId,
            componentType = componentType,
            propertyPath = propertyPath,
            value = value
        };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.ComponentSetProperty}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"Property '{propertyPath}' set to '{value}' successfully.";
    }
}
