using System.Text;
using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class GameObjectUseCase(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    public async ValueTask<string> FindAsync(string? query, CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var url = $"{baseUrl}{ApiRoutes.GameObjects}";
        if (!string.IsNullOrEmpty(query))
        {
            url += $"?query={Uri.EscapeDataString(query)}";
        }

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async ValueTask<string> CreateAsync(string name, CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new CreateGameObjectRequest { name = name };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.GameObjectCreate}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async ValueTask<string> DeleteAsync(int instanceId, CancellationToken cancellationToken)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        var request = new DeleteGameObjectRequest { instanceId = instanceId };
        var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.GameObjectDelete}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return $"GameObject {instanceId} deleted.";
    }

    public async ValueTask<string> ModifyAsync(int instanceId, string? name = null, bool? activeSelf = null,
        string? tag = null, int? layer = null, int? parentInstanceId = null,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = urlProvider.GetUrl();
        await EditorUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);

        // Use Dictionary instead of the shared ModifyGameObjectRequest DTO because
        // Unity's JsonUtility does not support Nullable<T>. The shared DTO uses
        // non-nullable value types (bool, int), so serializing it would always
        // include default values (false, 0) for unset fields. The Unity-side handler
        // detects field presence via string matching, which would misinterpret these
        // defaults as intentionally provided values.
        var fields = new Dictionary<string, object> { ["instanceId"] = instanceId };
        if (name != null) fields["name"] = name;
        if (activeSelf.HasValue) fields["activeSelf"] = activeSelf.Value;
        if (tag != null) fields["tag"] = tag;
        if (layer.HasValue) fields["layer"] = layer.Value;
        if (parentInstanceId.HasValue) fields["parentInstanceId"] = parentInstanceId.Value;

        var body = JsonSerializer.Serialize(fields);
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response =
            await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.GameObjectModify}", content, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
        return "GameObject modified successfully.";
    }
}
