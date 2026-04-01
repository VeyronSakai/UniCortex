namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IEditorWindowOperations
    {
        void FocusSceneView();
        void FocusGameView();
        (int width, int height) GetGameViewSize();
    }
}
