using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Editor.Domains.Models;
using UniCortex.Mcp.Domains.Interfaces;
using UniCortex.Mcp.Extensions;
using UniCortex.Mcp.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ConsoleTools(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("UniCortex");

    [McpServerTool(ReadOnly = true),
     Description("Get console log entries from the Unity Editor."), UsedImplicitly]
    public async Task<CallToolResult> GetConsoleLogs(
        [Description("Number of recent log entries to retrieve. Defaults to 100.")]
        int? count = null,
        [Description("Include stack traces in the output. Defaults to false.")]
        bool? stackTrace = null,
        [Description("Include Info-level logs. Defaults to true.")]
        bool? log = null,
        [Description("Include Warning-level logs. Defaults to true.")]
        bool? warning = null,
        [Description("Include Error-level logs. Defaults to true.")]
        bool? error = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var queryParams = new List<string>();
            if (count.HasValue) queryParams.Add($"count={count.Value}");
            if (stackTrace.HasValue) queryParams.Add($"stackTrace={stackTrace.Value.ToString().ToLowerInvariant()}");
            if (log.HasValue) queryParams.Add($"log={log.Value.ToString().ToLowerInvariant()}");
            if (warning.HasValue) queryParams.Add($"warning={warning.Value.ToString().ToLowerInvariant()}");
            if (error.HasValue) queryParams.Add($"error={error.Value.ToString().ToLowerInvariant()}");

            var url = $"{baseUrl}{ApiRoutes.ConsoleLogs}";
            if (queryParams.Count > 0)
            {
                url = $"{url}?{string.Join("&", queryParams)}";
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
     Description("Clear all console logs in the Unity Editor."), UsedImplicitly]
    public async Task<CallToolResult> ClearConsoleLogs(CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var response =
                await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.ConsoleClear}", null, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = "Console logs cleared successfully." }]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }
}
