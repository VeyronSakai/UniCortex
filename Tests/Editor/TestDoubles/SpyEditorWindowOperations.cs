using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyEditorWindowOperations : IEditorWindowOperations
    {
        public int FocusSceneViewCallCount { get; private set; }
        public int FocusGameViewCallCount { get; private set; }

        public void FocusSceneView()
        {
            FocusSceneViewCallCount++;
        }

        public void FocusGameView()
        {
            FocusGameViewCallCount++;
        }
    }
}
