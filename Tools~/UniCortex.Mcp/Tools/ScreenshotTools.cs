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
public class ScreenshotTools(IHttpClientFactory httpClientFactory, IUnityServerUrlProvider urlProvider)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("UniCortex");

    [McpServerTool(ReadOnly = true),
     Description("Capture a screenshot of the Game View as a PNG image."), UsedImplicitly]
    public async Task<CallToolResult> CaptureScreenshot(CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var response = await _httpClient.GetAsync($"{baseUrl}{ApiRoutes.ScreenshotCapture}", cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
            var pngData = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            return new CallToolResult
            {
                Content = [new ImageContentBlock { Data = pngData, MimeType = "image/png" }]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }
}
