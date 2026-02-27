using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyUtilityOperations : IUtilityOperations
    {
        public int ExecuteMenuItemCallCount { get; private set; }
        public string LastMenuPath { get; private set; }
        public bool ExecuteMenuItemResult { get; set; } = true;

        public int CaptureScreenshotCallCount { get; private set; }
        public byte[] ScreenshotResult { get; set; } = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        public bool ExecuteMenuItem(string menuPath)
        {
            ExecuteMenuItemCallCount++;
            LastMenuPath = menuPath;
            return ExecuteMenuItemResult;
        }

        public byte[] CaptureScreenshot()
        {
            CaptureScreenshotCallCount++;
            return ScreenshotResult;
        }
    }
}
