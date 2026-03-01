using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyScreenshotOperations : IScreenshotOperations
    {
        public int CaptureScreenshotCallCount { get; private set; }
        public byte[] ScreenshotResult { get; set; } = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        public byte[] CaptureScreenshot()
        {
            CaptureScreenshotCallCount++;
            return ScreenshotResult;
        }
    }
}
