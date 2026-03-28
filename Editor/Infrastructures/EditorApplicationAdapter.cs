using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class EditorApplicationAdapter : IEditorApplication
    {
        public bool IsPlaying
        {
            get => EditorApplication.isPlaying;
            set => EditorApplication.isPlaying = value;
        }

        public bool IsPaused
        {
            get => EditorApplication.isPaused;
            set => EditorApplication.isPaused = value;
        }

        public int ScreenWidth => (int)Handles.GetMainGameViewSize().x;
        public int ScreenHeight => (int)Handles.GetMainGameViewSize().y;

        public void Step()
        {
            EditorApplication.Step();
        }
    }
}
