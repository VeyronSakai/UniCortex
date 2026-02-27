using System.ComponentModel;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Domains.Interfaces;
using UniCortex.Mcp.Extensions;
using UniCortex.Mcp.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class SceneTools(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("UniCortex");

    [McpServerTool(ReadOnly = false),
     Description("Open a scene in the Unity Editor by its asset path."), UsedImplicitly]
    public async Task<CallToolResult> OpenScene(
        [Description("The asset path of the scene to open (e.g. \"Assets/Scenes/Main.unity\").")]
        string scenePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var body = JsonSerializer.Serialize(new OpenSceneRequest { scenePath = scenePath }, s_jsonOptions);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.SceneOpen}", content, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"Scene opened: {scenePath}" }]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }

    [McpServerTool(ReadOnly = false),
     Description("Save all open scenes in the Unity Editor."), UsedImplicitly]
    public async Task<CallToolResult> SaveScene(CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var response =
                await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.SceneSave}", null, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = "Scene saved successfully." }]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }

    [McpServerTool(ReadOnly = true),
     Description("Get the GameObject hierarchy of the current scene in the Unity Editor."), UsedImplicitly]
    public async Task<CallToolResult> GetSceneHierarchy(CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var response = await _httpClient.GetAsync($"{baseUrl}{ApiRoutes.SceneHierarchy}", cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            return new CallToolResult { Content = [new TextContentBlock { Text = json }] };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }
}
