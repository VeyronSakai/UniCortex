using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ScreenshotUseCase(IUnityEditorClient client)
{
    public ValueTask<byte[]> CaptureAsync(CancellationToken cancellationToken)
    {
        return client.GetBytesAsync(ApiRoutes.ScreenshotCapture, cancellationToken);
    }
}
