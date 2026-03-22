using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyCaptureOperations : ICaptureOperations
    {
        public int CaptureGameViewCallCount { get; private set; }
        public int CaptureSceneViewCallCount { get; private set; }
        public byte[] ScreenshotResult { get; set; } = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        public byte[] CaptureGameView()
        {
            CaptureGameViewCallCount++;
            return ScreenshotResult;
        }

        public byte[] CaptureSceneView()
        {
            CaptureSceneViewCallCount++;
            return ScreenshotResult;
        }
    }
}
