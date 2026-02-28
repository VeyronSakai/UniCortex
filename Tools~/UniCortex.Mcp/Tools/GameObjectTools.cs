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
public class GameObjectTools(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("UniCortex");

    [McpServerTool(ReadOnly = true),
     Description(
         "Find GameObjects in the current scene by name, tag, or component type. " +
         "Supports Unity Search style query syntax: plain text for name (partial match), " +
         "t:Type for component type, tag:partial or tag=exact for tag, id:N for instance ID, " +
         "layer:N for layer, path:A/B for hierarchy path, is:root/child/leaf/static for state filters."),
     UsedImplicitly]
    public async Task<CallToolResult> GetGameObjects(
        [Description(
            "Search query. Examples: 'Main Camera', 't:Camera', 'tag=Player', 'id:12345', 'is:root', 'path:Canvas/Button'. " +
            "Multiple tokens can be combined: 'Camera t:Camera layer:0'.")]
        string? query = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var url = $"{baseUrl}{ApiRoutes.GameObjects}";
            if (!string.IsNullOrEmpty(query))
            {
                url += $"?query={Uri.EscapeDataString(query)}";
            }

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
     Description("Create a new GameObject in the current scene. Supports primitive types (Cube, Sphere, etc.)."),
     UsedImplicitly]
    public async Task<CallToolResult> CreateGameObject(
        [Description("Name of the GameObject to create.")] string name,
        [Description("Primitive type: Cube, Sphere, Capsule, Cylinder, Plane, Quad. Omit for empty GameObject.")]
        string? primitive = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var request = new CreateGameObjectRequest { name = name, primitive = primitive ?? "" };
            var body = JsonSerializer.Serialize(request, s_jsonOptions);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response =
                await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.GameObjectCreate}", content, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            return new CallToolResult { Content = [new TextContentBlock { Text = json }] };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }

    [McpServerTool(ReadOnly = false), Description("Remove a GameObject from the current scene by its instance ID. Supports Undo."), UsedImplicitly]
    public async Task<CallToolResult> DeleteGameObject(
        [Description("The instance ID of the GameObject to delete.")]
        int instanceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var request = new DeleteGameObjectRequest { instanceId = instanceId };
            var body = JsonSerializer.Serialize(request, s_jsonOptions);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response =
                await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.GameObjectDelete}", content, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"GameObject {instanceId} deleted." }]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }

    [McpServerTool(ReadOnly = false),
     Description(
         "Modify a GameObject's properties (name, active state, tag, layer, parent). Only specified fields are changed."),
     UsedImplicitly]
    public async Task<CallToolResult> ModifyGameObject(
        [Description("The instance ID of the GameObject to modify.")]
        int instanceId,
        [Description("New name for the GameObject.")] string? name = null,
        [Description("Set active state (true/false).")] bool? activeSelf = null,
        [Description("New tag for the GameObject.")] string? tag = null,
        [Description("New layer index.")] int? layer = null,
        [Description("Instance ID of the new parent. Use 0 to move to root.")]
        int? parentInstanceId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            // Build JSON with only the provided fields
            var fields = new Dictionary<string, object> { ["instanceId"] = instanceId };
            if (name != null) fields["name"] = name;
            if (activeSelf.HasValue) fields["activeSelf"] = activeSelf.Value;
            if (tag != null) fields["tag"] = tag;
            if (layer.HasValue) fields["layer"] = layer.Value;
            if (parentInstanceId.HasValue) fields["parentInstanceId"] = parentInstanceId.Value;

            var body = JsonSerializer.Serialize(fields);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response =
                await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.GameObjectModify}", content, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = "GameObject modified successfully." }]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }
}
