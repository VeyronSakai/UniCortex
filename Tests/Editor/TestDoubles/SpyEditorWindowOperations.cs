using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyEditorWindowOperations : IEditorWindowOperations
    {
        public int FocusSceneViewCallCount { get; private set; }
        public int FocusGameViewCallCount { get; private set; }
        public int GetGameViewSizeCallCount { get; private set; }
        public int GameViewWidth { get; set; } = 800;
        public int GameViewHeight { get; set; } = 600;

        public void FocusSceneView()
        {
            FocusSceneViewCallCount++;
        }

        public void FocusGameView()
        {
            FocusGameViewCallCount++;
        }

        public (int width, int height) GetGameViewSize()
        {
            GetGameViewSizeCallCount++;
            return (GameViewWidth, GameViewHeight);
        }
    }
}
