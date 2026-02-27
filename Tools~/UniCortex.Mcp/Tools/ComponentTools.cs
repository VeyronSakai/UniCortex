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
public class ComponentTools(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("UniCortex");

    [McpServerTool(ReadOnly = false),
     Description("Add a component to a GameObject. Supports Undo."), UsedImplicitly]
    public async Task<CallToolResult> AddComponent(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description("The component type name to add (e.g. Rigidbody, BoxCollider).")]
        string componentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var request = new AddComponentRequest { instanceId = instanceId, componentType = componentType };
            var body = JsonSerializer.Serialize(request, s_jsonOptions);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response =
                await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.ComponentAdd}", content, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"Component '{componentType}' added successfully." }]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }

    [McpServerTool(ReadOnly = false),
     Description("Remove a component from a GameObject. Supports Undo."), UsedImplicitly]
    public async Task<CallToolResult> RemoveComponent(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description("The component type name to remove.")]
        string componentType,
        [Description("Index when multiple components of same type exist. Defaults to 0.")]
        int componentIndex = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var request = new RemoveComponentRequest
            {
                instanceId = instanceId, componentType = componentType, componentIndex = componentIndex
            };
            var body = JsonSerializer.Serialize(request, s_jsonOptions);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response =
                await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.ComponentRemove}", content, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"Component '{componentType}' removed successfully." }]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }

    [McpServerTool(ReadOnly = true),
     Description("Get serialized properties of a component on a GameObject."), UsedImplicitly]
    public async Task<CallToolResult> GetComponentProperties(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description("The component type name (e.g. Transform, Rigidbody).")]
        string componentType,
        [Description("Index when multiple components of same type exist. Defaults to 0.")]
        int componentIndex = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var url =
                $"{baseUrl}{ApiRoutes.ComponentProperties}?instanceId={instanceId}&componentType={Uri.EscapeDataString(componentType)}&componentIndex={componentIndex}";
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
     Description("Set a serialized property on a component. Uses SerializedProperty API with automatic Undo."),
     UsedImplicitly]
    public async Task<CallToolResult> SetComponentProperty(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description("The component type name.")] string componentType,
        [Description("The property path (e.g. m_LocalPosition.x).")]
        string propertyPath,
        [Description("The value as a string. Type is auto-detected from the property.")]
        string value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var request = new SetComponentPropertyRequest
            {
                instanceId = instanceId,
                componentType = componentType,
                propertyPath = propertyPath,
                value = value
            };
            var body = JsonSerializer.Serialize(request, s_jsonOptions);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response =
                await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.ComponentSetProperty}", content, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

            return new CallToolResult
            {
                Content =
                [
                    new TextContentBlock { Text = $"Property '{propertyPath}' set to '{value}' successfully." }
                ]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }
}
