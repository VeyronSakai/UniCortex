using System;
using System.Reflection;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class EditorWindowOperationsAdapter : IEditorWindowOperations
    {
        private static readonly Type s_gameViewType =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");

        private static readonly Type s_gameViewSizesType =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSizes");

        private static readonly Type s_gameViewSizeType =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSize");

        private static readonly Type s_gameViewSizeTypeEnum =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSizeType");

        private const BindingFlags AllInstance =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private const BindingFlags AllStatic =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
            BindingFlags.FlattenHierarchy;

        public void FocusSceneView()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.Focus();
                return;
            }

            EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
        }

        public void FocusGameView()
        {
            if (s_gameViewType == null)
            {
                throw new InvalidOperationException(
                    "GameView type not found. This Unity version may not be supported.");
            }

            EditorWindow.FocusWindowIfItsOpen(s_gameViewType);
        }

        public (int width, int height) GetGameViewSize()
        {
            var size = Handles.GetMainGameViewSize();
            return ((int)size.x, (int)size.y);
        }

        public GetGameViewSizeListResponse GetGameViewSizeList()
        {
            EnsureGameViewTypesAvailable();

            var (group, totalCount) = GetSizeGroup();

            var getGameViewSizeMethod = GetMethodOrThrow(group.GetType(), "GetGameViewSize");
            var baseTextProperty = GetPropertyOrThrow(s_gameViewSizeType, "baseText");
            var sizeWidthProperty = GetPropertyOrThrow(s_gameViewSizeType, "width");
            var sizeHeightProperty = GetPropertyOrThrow(s_gameViewSizeType, "height");
            var sizeTypeProperty = GetPropertyOrThrow(s_gameViewSizeType, "sizeType");

            var entries = new GameViewSizeEntry[totalCount];
            for (var i = 0; i < totalCount; i++)
            {
                var size = getGameViewSizeMethod.Invoke(group, new object[] { i });
                entries[i] = new GameViewSizeEntry
                {
                    index = i,
                    name = (string)baseTextProperty.GetValue(size),
                    width = (int)sizeWidthProperty.GetValue(size),
                    height = (int)sizeHeightProperty.GetValue(size),
                    sizeType = sizeTypeProperty.GetValue(size).ToString()
                };
            }

            var selectedIndex = GetSelectedSizeIndex();

            return new GetGameViewSizeListResponse
            {
                sizes = entries,
                selectedIndex = selectedIndex
            };
        }

        public void SetGameViewSize(int index)
        {
            EnsureGameViewTypesAvailable();

            var (_, totalCount) = GetSizeGroup();
            if (index < 0 || index >= totalCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"Index {index} is out of range. Valid range: 0 to {totalCount - 1}.");
            }

            SetSelectedSizeIndex(index);
        }

        public (float scale, float minScale, float maxScale) GetGameViewScale()
        {
            EnsureGameViewTypesAvailable();

            var gameView = EditorWindow.GetWindow(s_gameViewType);
            var zoomArea = GetZoomArea(gameView);

            var scaleProperty = GetPropertyOrThrow(zoomArea.GetType(), "scale");
            var scale = (Vector2)scaleProperty.GetValue(zoomArea);

            var (minScale, maxScale) = GetScaleRange(gameView);

            // GameView's ZoomableArea is uniform-scale (scale.x == scale.y); use the
            // absolute value of the vertical scale, which is what the toolbar slider shows.
            return (Mathf.Abs(scale.y), minScale, maxScale);
        }

        public float SetGameViewScale(float scale)
        {
            EnsureGameViewTypesAvailable();

            var gameView = EditorWindow.GetWindow(s_gameViewType);
            var zoomArea = GetZoomArea(gameView);

            var (minScale, maxScale) = GetScaleRange(gameView);
            var clamped = Mathf.Clamp(scale, minScale, maxScale);

            // Focus the zoom on the center of the visible area, mirroring what the slider does.
            var shownAreaProperty = GetPropertyOrThrow(zoomArea.GetType(), "shownAreaInsideMargins");
            var area = (Rect)shownAreaProperty.GetValue(zoomArea);
            var focalPoint = area.position + area.size * 0.5f;

            var setScaleFocused = GetMethodOrThrow(zoomArea.GetType(), "SetScaleFocused",
                new[] { typeof(Vector2), typeof(Vector2) });
            setScaleFocused.Invoke(zoomArea, new object[] { focalPoint, Vector2.one * clamped });

            gameView.Repaint();
            return clamped;
        }

        private static object GetZoomArea(EditorWindow gameView)
        {
            var zoomAreaField = GetFieldOrThrow(s_gameViewType, "m_ZoomArea");
            return zoomAreaField.GetValue(gameView)
                   ?? throw new InvalidOperationException(
                       "GameView's ZoomableArea is not initialized. This Unity version may not be supported.");
        }

        private static (float minScale, float maxScale) GetScaleRange(EditorWindow gameView)
        {
            var minScaleProperty = GetPropertyOrThrow(s_gameViewType, "minScale");
            var maxScaleProperty = GetPropertyOrThrow(s_gameViewType, "maxScale");
            var minScale = (float)minScaleProperty.GetValue(gameView);
            var maxScale = (float)maxScaleProperty.GetValue(gameView);
            return (minScale, maxScale);
        }

        private static void EnsureGameViewTypesAvailable()
        {
            if (s_gameViewType == null || s_gameViewSizesType == null ||
                s_gameViewSizeType == null || s_gameViewSizeTypeEnum == null)
            {
                throw new InvalidOperationException(
                    "Required internal GameView types not found. This Unity version may not be supported.");
            }
        }

        private static (object group, int totalCount) GetSizeGroup()
        {
            var singletonProperty = GetPropertyOrThrow(s_gameViewSizesType, "instance", AllStatic);
            var instance = singletonProperty.GetValue(null);

            var currentGroupMethod = GetMethodOrThrow(s_gameViewSizesType, "GetGroup");
            var currentGroupTypeProperty = GetPropertyOrThrow(s_gameViewSizesType, "currentGroupType");
            var currentGroupType = currentGroupTypeProperty.GetValue(instance);
            var group = currentGroupMethod.Invoke(instance, new[] { currentGroupType });

            var getTotalCountMethod = GetMethodOrThrow(group.GetType(), "GetTotalCount");
            var totalCount = (int)getTotalCountMethod.Invoke(group, null);

            return (group, totalCount);
        }

        private static int GetSelectedSizeIndex()
        {
            var gameView = EditorWindow.GetWindow(s_gameViewType);
            var selectedSizeIndexProp = GetPropertyOrThrow(s_gameViewType, "selectedSizeIndex");
            return (int)selectedSizeIndexProp.GetValue(gameView);
        }

        private static void SetSelectedSizeIndex(int index)
        {
            var gameView = EditorWindow.GetWindow(s_gameViewType);
            var selectedSizeIndexProp = GetPropertyOrThrow(s_gameViewType, "selectedSizeIndex");
            selectedSizeIndexProp.SetValue(gameView, index);
        }

        private static MethodInfo GetMethodOrThrow(Type type, string name)
        {
            return type.GetMethod(name, AllInstance)
                   ?? throw new InvalidOperationException(
                       $"Unity internal API changed: method '{name}' was not found on {type.Name}. " +
                       "This Unity version may not be supported.");
        }

        private static MethodInfo GetMethodOrThrow(Type type, string name, Type[] parameterTypes)
        {
            return type.GetMethod(name, AllInstance, null, parameterTypes, null)
                   ?? throw new InvalidOperationException(
                       $"Unity internal API changed: method '{name}' was not found on {type.Name}. " +
                       "This Unity version may not be supported.");
        }

        private static FieldInfo GetFieldOrThrow(Type type, string name, BindingFlags flags = AllInstance)
        {
            return type.GetField(name, flags)
                   ?? throw new InvalidOperationException(
                       $"Unity internal API changed: field '{name}' was not found on {type.Name}. " +
                       "This Unity version may not be supported.");
        }

        private static PropertyInfo GetPropertyOrThrow(Type type, string name,
            BindingFlags flags = AllInstance)
        {
            return type.GetProperty(name, flags)
                   ?? throw new InvalidOperationException(
                       $"Unity internal API changed: property '{name}' was not found on {type.Name}. " +
                       "This Unity version may not be supported.");
        }
    }
}
