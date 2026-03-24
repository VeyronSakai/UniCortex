using System;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class CaptureOperationsAdapter : ICaptureOperations
    {
        public byte[] CaptureGameView()
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Game View capture is only available in Play Mode. Enter Play Mode first.");
            }

            // Use ScreenCapture to capture the full Game View output including UI overlays.
            // Camera.Render() only renders the camera's own view and misses Screen Space - Overlay canvases.
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
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
