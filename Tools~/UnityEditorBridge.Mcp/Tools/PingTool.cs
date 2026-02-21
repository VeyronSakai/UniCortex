using System.ComponentModel;
using EditorBridge.Editor.Domains.Models;
using ModelContextProtocol.Server;

namespace UnityEditorBridge.Mcp.Tools;

[McpServerToolType]
public class PingTool
{
    private readonly HttpClient _httpClient;

    public PingTool(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("EditorBridge");
    }

    [McpServerTool(ReadOnly = true), Description("Check connectivity with the Unity Editor.")]
    public async Task<string> Ping(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(ApiRoutes.Ping, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
