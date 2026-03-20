using UniCortex.Core.Infrastructures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ScreenshotUseCase(UnityEditorClient client)
{
    public ValueTask<byte[]> CaptureAsync(CancellationToken cancellationToken)
    {
        return client.GetBytesAsync(ApiRoutes.ScreenshotCapture, cancellationToken);
    }
}
