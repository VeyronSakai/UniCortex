using System;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class ScreenshotOperationsAdapter : IScreenshotOperations
    {
        public byte[] CaptureScreenshot()
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Screenshot capture is only available in Play Mode. Enter Play Mode first.");
            }

            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            if (texture == null)
            {
                throw new InvalidOperationException(
                    "Failed to capture screenshot. Ensure the Game View is open and a Camera is rendering.");
            }

            try
            {
                return texture.EncodeToPNG();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
