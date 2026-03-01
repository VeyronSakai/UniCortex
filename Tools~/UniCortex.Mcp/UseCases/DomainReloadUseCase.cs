using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Extensions;

namespace UniCortex.Mcp.UseCases;

internal static class DomainReloadUseCase
{
    internal static async Task ReloadAsync(HttpClient httpClient, string baseUrl, CancellationToken cancellationToken)
    {
        // Wait for the server to become available before triggering domain reload.
        // If Unity is already auto-recompiling (e.g. after a .cs file change),
        // the server will be unavailable; this prevents a double RequestScriptCompilation() call
        // that can freeze Unity.
        await WaitForServerAsync(httpClient, baseUrl, cancellationToken);

        var response = await httpClient.PostAsync($"{baseUrl}{ApiRoutes.DomainReload}", null, cancellationToken);
        await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

        // RequestScriptCompilation() is dispatched asynchronously on the Unity main thread.
        // Wait briefly so that compilation starts and the server becomes unavailable
        // before we begin polling /ping.
        await Task.Delay(1000, cancellationToken);

        await WaitForServerAsync(httpClient, baseUrl, cancellationToken);
    }

    /// <summary>
    /// Poll GET /ping until the server responds with a non-empty body.
    /// HttpRequestHandler handles retries for GET requests during domain reload.
    /// </summary>
    internal static async Task WaitForServerAsync(HttpClient httpClient, string baseUrl,
        CancellationToken cancellationToken)
    {
        var pingResponse = await httpClient.GetAsync($"{baseUrl}{ApiRoutes.Ping}", cancellationToken);
        await pingResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);
    }
}
