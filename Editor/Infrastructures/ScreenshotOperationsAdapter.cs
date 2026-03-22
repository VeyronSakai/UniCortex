using System;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class ScreenshotOperationsAdapter : IScreenshotOperations
    {
        public byte[] CaptureGameView()
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Game View capture is only available in Play Mode. Enter Play Mode first.");
            }

            var camera = FindGameViewCamera();
            if (camera == null)
            {
                throw new InvalidOperationException(
                    "No active camera found. Ensure at least one enabled Camera exists in the scene.");
            }

            return RenderCameraToPng(camera);
        }

        private static Camera FindGameViewCamera()
        {
            if (Camera.main != null)
            {
                return Camera.main;
            }

            // Fallback: pick the camera with the highest depth (rendered last = topmost in Game View).
            // Exclude cameras that render to a RenderTexture (they are off-screen cameras).
            return Camera.allCameras
                .Where(c => c.targetTexture == null)
                .OrderByDescending(c => c.depth)
                .FirstOrDefault();
        }

        public byte[] CaptureSceneView()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                throw new InvalidOperationException(
                    "No Scene View is open. Open a Scene View window first.");
            }

            return RenderCameraToPng(sceneView.camera);
        }

        private static byte[] RenderCameraToPng(Camera camera)
        {
            var width = camera.pixelWidth;
            var height = camera.pixelHeight;

            var rt = new RenderTexture(width, height, 24);
            var originalTarget = camera.targetTexture;
            var originalActive = RenderTexture.active;

            try
            {
                camera.targetTexture = rt;
                camera.Render();

                RenderTexture.active = rt;
                var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();

                try
                {
                    return texture.EncodeToPNG();
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }
            finally
            {
                camera.targetTexture = originalTarget;
                RenderTexture.active = originalActive;
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }
    }
}
