using System;
using System.Reflection;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class CaptureOperationsAdapter : ICaptureOperations
    {
        private static readonly Type s_gameViewType =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");

        // PlayModeView.m_TargetTexture holds the rendered game image at the Game View resolution.
        private static readonly FieldInfo s_targetTextureField =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PlayModeView")
                ?.GetField("m_TargetTexture", BindingFlags.Instance | BindingFlags.NonPublic);

        public byte[] CaptureGameView()
        {
            var targetTexture = GetGameViewTargetTexture();
            if (targetTexture == null)
            {
                throw new InvalidOperationException(
                    "Game View has not been rendered yet. Make sure the Game View is open and visible.");
            }

            // Read the Game View's own render target so that only the game image (including Screen Space -
            // Overlay UI) is captured at the Game View resolution, without the surrounding editor window.
            // On graphics APIs whose UV origin is at the top (Metal, Direct3D, Vulkan), the target is stored
            // upside down, so flip it back.
            return EncodeToPng(targetTexture, SystemInfo.graphicsUVStartsAtTop);
        }

        public byte[] CaptureSceneView()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null || sceneView.camera == null)
            {
                throw new InvalidOperationException("Scene View is not open. Open a Scene View first.");
            }

            var camera = sceneView.camera;
            var width = camera.pixelWidth;
            var height = camera.pixelHeight;
            if (width <= 0 || height <= 0)
            {
                throw new InvalidOperationException("Scene View has no visible area to capture.");
            }

            // Render the Scene View camera into an offscreen texture. This also works in Prefab Mode,
            // because the Scene View camera is bound to the preview scene of the Prefab stage.
            var renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var previousTarget = camera.targetTexture;
            try
            {
                camera.targetTexture = renderTexture;
                camera.Render();
                return EncodeToPng(renderTexture, false);
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }

        private static RenderTexture GetGameViewTargetTexture()
        {
            if (s_gameViewType == null || s_targetTextureField == null)
            {
                throw new InvalidOperationException(
                    "GameView internals not found. This Unity version may not be supported.");
            }

            var gameViews = Resources.FindObjectsOfTypeAll(s_gameViewType);
            if (gameViews.Length == 0)
            {
                throw new InvalidOperationException("Game View is not open. Open a Game View first.");
            }

            return s_targetTextureField.GetValue(gameViews[0]) as RenderTexture;
        }

        private static byte[] EncodeToPng(RenderTexture source, bool flipVertically)
        {
            var width = source.width;
            var height = source.height;
            var flipped = flipVertically
                ? RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32)
                : null;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            var previousActive = RenderTexture.active;
            try
            {
                if (flipped != null)
                {
                    Graphics.Blit(source, flipped, new Vector2(1f, -1f), new Vector2(0f, 1f));
                }

                RenderTexture.active = flipped != null ? flipped : source;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                return texture.EncodeToPNG();
            }
            finally
            {
                RenderTexture.active = previousActive;
                if (flipped != null)
                {
                    RenderTexture.ReleaseTemporary(flipped);
                }

                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
