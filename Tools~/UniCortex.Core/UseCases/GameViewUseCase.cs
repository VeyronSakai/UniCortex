using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class GameViewUseCase(IUnityEditorClient client)
{
    public async ValueTask<byte[]> CaptureAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync<CaptureGameViewRequest, CaptureGameViewResponse>(
            ApiRoutes.GameViewCapture, cancellationToken: cancellationToken);
        return Convert.FromBase64String(response.pngDataBase64);
    }
}
