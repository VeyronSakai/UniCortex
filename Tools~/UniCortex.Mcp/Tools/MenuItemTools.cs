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
public class MenuItemTools(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("UniCortex");

    [McpServerTool(ReadOnly = false),
     Description("Execute a Unity Editor menu item by its path (e.g. \"GameObject/3D Object/Cube\")."),
     UsedImplicitly]
    public async Task<CallToolResult> ExecuteMenuItem(
        [Description("The full menu path (e.g. \"GameObject/3D Object/Cube\", \"File/Save\").")]
        string menuPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var request = new ExecuteMenuItemRequest { menuPath = menuPath };
            var body = JsonSerializer.Serialize(request, s_jsonOptions);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response =
                await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.MenuItemExecute}", content, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"Menu item executed: {menuPath}" }]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }
}
