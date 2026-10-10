using System;
using System.Reflection;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class CaptureOperationsAdapter : ICaptureOperations
    {
        // PlayModeView.m_TargetTexture holds the rendered game image at the Game View resolution.
        private static readonly FieldInfo? s_targetTextureField =
            PlayModeViewUtility.PlayModeViewType
                ?.GetField("m_TargetTexture", BindingFlags.Instance | BindingFlags.NonPublic);

        // Same color as the safe area highlight of the Simulator view.
        private static readonly Color s_safeAreaColor = new Color(0.95f, 1f, 0f);
        private static readonly Color s_cutoutColor = new Color(1f, 0f, 0f, 0.5f);

        public byte[] CaptureGameView(GetScreenSafeAreaResponse? safeAreaToDraw, bool drawDeviceFrame)
        {
            var deviceFrame = drawDeviceFrame ? PlayModeViewUtility.GetDeviceFrame() : null;
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
            return EncodeToPng(targetTexture, SystemInfo.graphicsUVStartsAtTop, safeAreaToDraw, deviceFrame);
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
            // camera.targetTexture only specifies where to render and is null outside the Scene View's own
            // drawing (a null target renders to whatever is active, which cannot be read back), so render
            // into a temporary texture that can be read back.
            var renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var previousTarget = camera.targetTexture;
            try
            {
                camera.targetTexture = renderTexture;
                camera.Render();
                return EncodeToPng(renderTexture, false, null, null);
            }
            finally
            {
                // Restore the original target so the camera does not keep pointing at the released texture.
                // Restore the saved value rather than null, to leave the camera exactly as it was.
                camera.targetTexture = previousTarget;
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }

        private static RenderTexture? GetGameViewTargetTexture()
        {
            if (s_targetTextureField == null)
            {
                throw new InvalidOperationException(
                    "GameView internals not found. This Unity version may not be supported.");
            }

            // The main Play Mode view is the Game view or the Simulator view that the game renders to.
            // The capture use case has just opened or focused it.
            var playModeView = PlayModeViewUtility.GetMainPlayModeView();
            if (playModeView == null)
            {
                throw new InvalidOperationException("Game View is not open. Open a Game View first.");
            }

            return s_targetTextureField.GetValue(playModeView) as RenderTexture;
        }

        private static byte[] EncodeToPng(RenderTexture source, bool flipVertically,
            GetScreenSafeAreaResponse? safeAreaToDraw, DeviceFrame? deviceFrame)
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
                if (safeAreaToDraw != null)
                {
                    DrawSafeArea(texture, safeAreaToDraw);
                }

                if (deviceFrame != null)
                {
                    var framed = DeviceFrameRenderer.Render(texture, deviceFrame);
                    try
                    {
                        return framed.EncodeToPNG();
                    }
                    finally
                    {
                        UnityEngine.Object.DestroyImmediate(framed);
                    }
                }

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

        // Draws the cutouts filled with translucent red and the outline of the safe area onto the texture.
        // Both are in screen coordinates (origin at the bottom-left), the same as the texture's pixel coordinates.
        private static void DrawSafeArea(Texture2D texture, GetScreenSafeAreaResponse safeArea)
        {
            // The screen size may differ slightly from the texture size (e.g. fractional Game View sizes).
            var scaleX = safeArea.screenWidth > 0 ? (float)texture.width / safeArea.screenWidth : 1f;
            var scaleY = safeArea.screenHeight > 0 ? (float)texture.height / safeArea.screenHeight : 1f;

            foreach (var cutout in safeArea.cutouts)
            {
                var rect = ToPixelRect(texture, cutout, scaleX, scaleY);
                BlendRect(texture, rect, s_cutoutColor);
            }

            var area = ToPixelRect(texture, safeArea.safeArea, scaleX, scaleY);
            var thickness = Mathf.Max(2, Mathf.RoundToInt(Mathf.Min(texture.width, texture.height) / 200f));
            BlendRect(texture, new RectInt(area.xMin, area.yMin, area.width, thickness), s_safeAreaColor);
            BlendRect(texture, new RectInt(area.xMin, area.yMax - thickness, area.width, thickness),
                s_safeAreaColor);
            BlendRect(texture, new RectInt(area.xMin, area.yMin, thickness, area.height), s_safeAreaColor);
            BlendRect(texture, new RectInt(area.xMax - thickness, area.yMin, thickness, area.height),
                s_safeAreaColor);
        }

        private static RectInt ToPixelRect(Texture2D texture, ScreenRect rect, float scaleX, float scaleY)
        {
            var xMin = Mathf.Clamp(Mathf.RoundToInt(rect.x * scaleX), 0, texture.width);
            var yMin = Mathf.Clamp(Mathf.RoundToInt(rect.y * scaleY), 0, texture.height);
            var xMax = Mathf.Clamp(Mathf.RoundToInt((rect.x + rect.width) * scaleX), 0, texture.width);
            var yMax = Mathf.Clamp(Mathf.RoundToInt((rect.y + rect.height) * scaleY), 0, texture.height);
            return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        private static void BlendRect(Texture2D texture, RectInt rect, Color color)
        {
            var xMin = Mathf.Clamp(rect.xMin, 0, texture.width);
            var yMin = Mathf.Clamp(rect.yMin, 0, texture.height);
            var xMax = Mathf.Clamp(rect.xMax, 0, texture.width);
            var yMax = Mathf.Clamp(rect.yMax, 0, texture.height);
            if (xMax <= xMin || yMax <= yMin)
            {
                return;
            }

            var blockWidth = xMax - xMin;
            var blockHeight = yMax - yMin;
            var pixels = texture.GetPixels(xMin, yMin, blockWidth, blockHeight);
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.Lerp(pixels[i], color, color.a);
            }

            texture.SetPixels(xMin, yMin, blockWidth, blockHeight, pixels);
        }
    }
}
