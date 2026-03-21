using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ScreenshotUseCase(IUnityEditorClient client)
{
    public async ValueTask<byte[]> CaptureAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync<CaptureScreenshotRequest, CaptureScreenshotResponse>(
            ApiRoutes.ScreenshotCapture, cancellationToken: cancellationToken);
        return Convert.FromBase64String(response.pngDataBase64);
    }
}
