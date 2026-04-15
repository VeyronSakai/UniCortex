using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IEditorWindowOperations
    {
        void FocusSceneView();
        void SetSceneViewCamera(SetSceneViewCameraRequest request);
        void FocusGameView();
        (int width, int height) GetGameViewSize();
        GetGameViewSizeListResponse GetGameViewSizeList();
        void SetGameViewSize(int index);
    }
}
