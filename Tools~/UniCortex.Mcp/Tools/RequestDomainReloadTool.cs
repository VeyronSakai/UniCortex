using System.ComponentModel;
using UniCortex.Editor.Domains.Models;
using JetBrains.Annotations;
using ModelContextProtocol.Server;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class RequestDomainReloadTool(IHttpClientFactory httpClientFactory)
{
    [McpServerTool(ReadOnly = false), Description("Request a domain reload (script recompilation) in the Unity Editor."), UsedImplicitly]
    public async Task<string> RequestDomainReload(CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient("UniCortex");

        var response = await httpClient.PostAsync(ApiRoutes.DomainReload, null, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Poll /ping to wait for the server to come back after domain reload.
        // DomainReloadRetryHandler handles retries during the reload.
        var pingResponse = await httpClient.GetAsync(ApiRoutes.Ping, cancellationToken);
        pingResponse.EnsureSuccessStatusCode();

        return "Domain reload completed successfully.";
    }
}
