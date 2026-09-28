using System;
using System.Reflection;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEditor.SceneManagement;
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
            if (sceneView == null)
            {
                throw new InvalidOperationException("Scene View is not open. Open a Scene View first.");
            }

            var (width, height) = GetSceneViewPixelSize(sceneView);
            if (width <= 0 || height <= 0)
            {
                throw new InvalidOperationException("Scene View has no visible area to capture.");
            }

            // The Scene View camera is only set up while the Scene View draws itself, so it keeps default values
            // (e.g. at the origin) until the Scene View is shown after a domain reload such as entering Play Mode.
            // Build a temporary camera from the Scene View's own view state instead.
            var cameraObject = new GameObject("UniCortexSceneViewCapture") { hideFlags = HideFlags.HideAndDontSave };
            var renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            try
            {
                var camera = cameraObject.AddComponent<Camera>();
                camera.enabled = false;
                SetupSceneViewCamera(camera, sceneView, (float)width / height);

                // In Prefab Mode, render the Prefab stage's preview scene instead of the main scenes.
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null)
                {
                    camera.scene = prefabStage.scene;
                }

                camera.targetTexture = renderTexture;
                camera.Render();
                camera.targetTexture = null;
                return EncodeToPng(renderTexture, false);
            }
            finally
            {
                RenderTexture.ReleaseTemporary(renderTexture);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        private static (int width, int height) GetSceneViewPixelSize(SceneView sceneView)
        {
            // cameraViewport is NaN until the Scene View has been drawn, so fall back to the window size.
            var viewport = sceneView.cameraViewport;
            var size = float.IsNaN(viewport.width) || float.IsNaN(viewport.height)
                ? sceneView.position.size
                : viewport.size;
            var pixelsPerPoint = EditorGUIUtility.pixelsPerPoint;
            return (Mathf.RoundToInt(size.x * pixelsPerPoint), Mathf.RoundToInt(size.y * pixelsPerPoint));
        }

        private static void SetupSceneViewCamera(Camera camera, SceneView sceneView, float aspect)
        {
            // Mirrors how the Scene View sets up its camera from pivot, rotation and size.
            var rotation = sceneView.rotation;
            camera.transform.SetPositionAndRotation(
                sceneView.pivot - rotation * Vector3.forward * sceneView.cameraDistance, rotation);
            camera.aspect = aspect;

            var settings = sceneView.cameraSettings;
            camera.orthographic = sceneView.orthographic;
            camera.orthographicSize = sceneView.size;
            camera.fieldOfView = settings.fieldOfView;

            if (settings.dynamicClip)
            {
                camera.nearClipPlane = sceneView.size * 0.01f;
                camera.farClipPlane = sceneView.size * 2000f;
            }
            else
            {
                camera.nearClipPlane = settings.nearClip;
                camera.farClipPlane = settings.farClip;
            }

            if (sceneView.sceneViewState.showSkybox)
            {
                camera.clearFlags = CameraClearFlags.Skybox;
            }
            else
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = sceneView.camera != null ? sceneView.camera.backgroundColor : Color.gray;
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
