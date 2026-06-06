using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IEditorWindowOperations
    {
        void FocusSceneView();
        void FocusGameView();
        (int width, int height) GetGameViewSize();
        GetGameViewSizeListResponse GetGameViewSizeList();
        void SetGameViewSize(int index);
        (float scale, float minScale, float maxScale) GetGameViewScale();
        float SetGameViewScale(float scale);
    }
}
