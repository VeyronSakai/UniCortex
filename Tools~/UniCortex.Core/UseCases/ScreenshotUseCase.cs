using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ScreenshotUseCase(IUnityEditorClient client)
{
    public async ValueTask<byte[]> CaptureGameViewAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync<CaptureGameViewRequest, CaptureGameViewResponse>(
            ApiRoutes.GameViewCapture, cancellationToken: cancellationToken);
        return Convert.FromBase64String(response.pngDataBase64);
    }

    public async ValueTask<byte[]> CaptureSceneViewAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync<CaptureSceneViewRequest, CaptureSceneViewResponse>(
            ApiRoutes.SceneViewCapture, cancellationToken: cancellationToken);
        return Convert.FromBase64String(response.pngDataBase64);
    }
}
