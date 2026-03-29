using System;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class CaptureOperationsAdapter : ICaptureOperations
    {
        public byte[] CaptureScreenshot()
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Screenshot capture is only available in Play Mode. Enter Play Mode first.");
            }

            // Use ScreenCapture to capture the current rendering output including UI overlays.
            // Camera.Render() only renders the camera's own view and misses Screen Space - Overlay canvases.
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
