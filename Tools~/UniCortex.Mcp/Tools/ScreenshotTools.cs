using System.ComponentModel;
using System.Text;
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
     Description("Capture a screenshot of the Game View as a PNG image. Only available in Play Mode."),
     UsedImplicitly]
    public async Task<CallToolResult> CaptureScreenshot(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = urlProvider.GetUrl();
            await DomainReloadUseCase.ReloadAsync(_httpClient, baseUrl, cancellationToken);

            var url = $"{baseUrl}{ApiRoutes.ScreenshotCapture}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            await response.EnsureSuccessWithErrorBodyAsync(cancellationToken);
            var pngData = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            // ImageContentBlock.Data expects Base64-encoded string as UTF-8 bytes.
            var base64Data = Encoding.UTF8.GetBytes(Convert.ToBase64String(pngData));

            return new CallToolResult
            {
                Content = [new ImageContentBlock { Data = base64Data, MimeType = "image/png" }]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = ex.ToString() }] };
        }
    }
}
