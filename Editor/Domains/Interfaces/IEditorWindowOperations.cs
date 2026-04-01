using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IEditorWindowOperations
    {
        void FocusSceneView();
        void FocusGameView();
        (int width, int height) GetGameViewSize();
        GetGameViewSizeListResponse GetGameViewSizeList();
        void SetGameViewSizeByIndex(int index);
        void SetGameViewSize(int width, int height);
    }
}
