using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Extensions;

namespace UniCortex.Mcp.UseCases;

internal static class DomainReloadUseCase
{
    internal static async Task ReloadAsync(HttpClient httpClient, string baseUrl, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsync(baseUrl + ApiRoutes.DomainReload, null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        // Poll /ping to wait for the server to come back after domain reload.
        // DomainReloadRetryHandler handles retries during the reload.
        var pingResponse = await httpClient.GetAsync(baseUrl + ApiRoutes.Ping, cancellationToken);
        await pingResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);
    }
}
