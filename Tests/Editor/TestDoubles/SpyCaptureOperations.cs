using System;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyCaptureOperations : ICaptureOperations
    {
        public int CaptureGameViewCallCount { get; private set; }
        public GetScreenSafeAreaResponse LastSafeAreaToDraw { get; private set; }
        public bool LastDrawDeviceFrame { get; private set; }
        public byte[] GameViewResult { get; set; } = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        public int CaptureSceneViewCallCount { get; private set; }
        public byte[] SceneViewResult { get; set; } = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        public Exception ExceptionToThrow { get; set; }

        public byte[] CaptureGameView(GetScreenSafeAreaResponse safeAreaToDraw, bool drawDeviceFrame)
        {
            CaptureGameViewCallCount++;
            LastSafeAreaToDraw = safeAreaToDraw;
            LastDrawDeviceFrame = drawDeviceFrame;
            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            return GameViewResult;
        }

        public byte[] CaptureSceneView()
        {
            CaptureSceneViewCallCount++;
            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            return SceneViewResult;
        }
    }
}
