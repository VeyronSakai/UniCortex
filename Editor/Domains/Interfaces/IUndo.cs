namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IUndo
    {
        void PerformUndo();
        void PerformRedo();
    }
}
