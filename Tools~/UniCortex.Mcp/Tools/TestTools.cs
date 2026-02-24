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

            var response = await _httpClient.PostAsync($"{baseUrl}{ApiRoutes.TestsRun}", content,
                cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            return new CallToolResult { Content = [new TextContentBlock { Text = responseJson }] };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }
}
