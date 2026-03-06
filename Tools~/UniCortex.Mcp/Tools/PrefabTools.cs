using System.ComponentModel;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Domains;
using UniCortex.Mcp.Domains.Interfaces;
using UniCortex.Mcp.Extensions;
using UniCortex.Mcp.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class PrefabTools(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.UniCortex);

    [McpServerTool(Name = "create_prefab", ReadOnly = false),
     Description("Create a Prefab asset from a GameObject in the scene."), UsedImplicitly]
    public async ValueTask<CallToolResult> CreatePrefabAsync(
        [Description("The instance ID of the GameObject to save as a Prefab.")]
        int instanceId,
        [Description("The asset path where the Prefab should be saved (e.g. \"Assets/Prefabs/MyCube.prefab\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var request = new CreatePrefabRequest { instanceId = instanceId, assetPath = assetPath };
            var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response =
                await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.PrefabCreate}", content, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"Prefab created at: {assetPath}" }]
            };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "instantiate_prefab", ReadOnly = false),
     Description("Instantiate a Prefab into the current scene. Returns the new GameObject's name and instance ID."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> InstantiatePrefabAsync(
        [Description("The asset path of the Prefab to instantiate (e.g. \"Assets/Prefabs/MyCube.prefab\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var request = new InstantiatePrefabRequest { assetPath = assetPath };
            var body = JsonSerializer.Serialize(request, new JsonSerializerOptions { IncludeFields = true });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response =
                await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.PrefabInstantiate}", content, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            return new CallToolResult { Content = [new TextContentBlock { Text = json }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
