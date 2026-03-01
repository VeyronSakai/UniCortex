using UniCortex.Editor.Domains.Interfaces;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class ScreenshotOperationsAdapter : IScreenshotOperations
    {
        public byte[] CaptureScreenshot()
        {
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                return texture.EncodeToPNG();
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }
}
