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

        public int ScreenWidth => Screen.width;
        public int ScreenHeight => Screen.height;

        public void Step()
        {
            EditorApplication.Step();
        }
    }
}
