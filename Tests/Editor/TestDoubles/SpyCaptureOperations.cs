using System;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyCaptureOperations : ICaptureOperations
    {
        public int CaptureGameViewCallCount { get; private set; }
        public byte[] GameViewResult { get; set; } = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        public int CaptureSceneViewCallCount { get; private set; }
        public byte[] SceneViewResult { get; set; } = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        public Exception ExceptionToThrow { get; set; }

        public byte[] CaptureGameView()
        {
            CaptureGameViewCallCount++;
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
