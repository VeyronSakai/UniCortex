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
public class ScriptableObjectTools(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("UniCortex");

    [McpServerTool(ReadOnly = false),
     Description("Create a new ScriptableObject asset at the specified path."), UsedImplicitly]
    public async Task<CallToolResult> CreateScriptableObject(
        [Description("The ScriptableObject subclass type name to create.")]
        string type,
        [Description("The asset path where the asset should be created (e.g. \"Assets/Materials/NewMat.mat\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var request = new CreateScriptableObjectRequest { type = type, assetPath = assetPath };
            var body = JsonSerializer.Serialize(request, s_jsonOptions);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response =
                await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.ScriptableObjectCreate}", content, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"ScriptableObject created at: {assetPath}" }]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }

    [McpServerTool(ReadOnly = true),
     Description("Get information and serialized properties of a ScriptableObject asset."), UsedImplicitly]
    public async Task<CallToolResult> GetScriptableObjectInfo(
        [Description("The asset path to inspect (e.g. \"Assets/Data/MyConfig.asset\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var url = $"{baseUrl}{ApiRoutes.ScriptableObjectInfo}?assetPath={Uri.EscapeDataString(assetPath)}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            return new CallToolResult { Content = [new TextContentBlock { Text = json }] };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }

    [McpServerTool(ReadOnly = false),
     Description("Set a serialized property on a ScriptableObject asset."), UsedImplicitly]
    public async Task<CallToolResult> SetScriptableObjectProperty(
        [Description("The asset path of the ScriptableObject to modify.")] string assetPath,
        [Description("The property path (e.g. \"m_Name\", \"myField\").")] string propertyPath,
        [Description("The new value as a string.")] string value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var request = new SetScriptableObjectPropertyRequest
                { assetPath = assetPath, propertyPath = propertyPath, value = value };
            var body = JsonSerializer.Serialize(request, s_jsonOptions);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response =
                await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.ScriptableObjectSetProperty}", content, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"Property '{propertyPath}' set on ScriptableObject '{assetPath}'." }]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }
}
