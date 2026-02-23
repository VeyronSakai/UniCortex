using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class UndoAdapter : IUndo
    {
        public void PerformUndo()
        {
            Undo.PerformUndo();
        }

        public void PerformRedo()
        {
            Undo.PerformRedo();
        }
    }
}
