using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyUndo : IUndo
    {
        public int PerformUndoCallCount { get; private set; }
        public int PerformRedoCallCount { get; private set; }

        public void PerformUndo()
        {
            PerformUndoCallCount++;
        }

        public void PerformRedo()
        {
            PerformRedoCallCount++;
        }
    }
}
