using System.ComponentModel;
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
public class TestTools(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("UniCortex");
    private readonly JsonSerializerOptions _jsonOptions = new() { IncludeFields = true };

    [McpServerTool(ReadOnly = true),
     Description("Run Unity Test Runner tests and wait for completion."), UsedImplicitly]
    public async Task<CallToolResult> RunTests(
        [Description("Test mode: 'EditMode' or 'PlayMode'. Defaults to 'EditMode'.")]
        string? testMode = null,
        [Description("Test name filter. Omit to run all tests.")]
        string? nameFilter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var request = new RunTestsRequest(testMode ?? "EditMode", nameFilter ?? "");
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            string responseJson;
            try
            {
                var response = await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.TestsRun}", content,
                    cancellationToken);
                await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
                responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (HttpRequestException)
            {
                // Server disrupted (e.g., domain reload during PlayMode entry)
                responseJson = "";
            }

            // PlayMode tests trigger a domain reload when entering play mode,
            // which disrupts the HTTP server. Poll GET /tests/result for results.
            // The HttpRequestHandler retries GET on Content-Length: 0, so this
            // automatically polls until results are stored in SessionState.
            if (string.IsNullOrEmpty(responseJson))
            {
                await DomainReloadUseCase.WaitForServerAsync(_httpClient, baseUrl, cancellationToken);
                var resultResponse =
                    await _httpClient.GetAsync($"{baseUrl}{ApiRoutes.TestsResult}", cancellationToken);
                await resultResponse.EnsureSuccessWithErrorBodyAsync(cancellationToken);
                responseJson = await resultResponse.Content.ReadAsStringAsync(cancellationToken);
            }

            return new CallToolResult { Content = [new TextContentBlock { Text = responseJson }] };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }
}
