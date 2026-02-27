using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class UtilityOperationsAdapter : IUtilityOperations
    {
        public bool ExecuteMenuItem(string menuPath)
        {
            return EditorApplication.ExecuteMenuItem(menuPath);
        }

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
