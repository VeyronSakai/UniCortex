using System;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class EditorWindowOperationsAdapter : IEditorWindowOperations
    {
        private static readonly Type s_gameViewType =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");

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
    }
}
