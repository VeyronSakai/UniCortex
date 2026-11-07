using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IEditorWindowOperations
    {
        void FocusSceneView();
        void FocusGameView();
        void OpenGameView();
        (int width, int height) GetGameViewSize();
        GetGameViewSizeListResponse GetGameViewSizeList();

        // The sizes of the current platform. Unlike GetGameViewSizeList, this does not open the Game View.
        GameViewSizeEntry[] GetGameViewSizes();

        // Adds a fixed resolution to the end of the size list of the current platform without selecting it,
        // and returns its index.
        int AddGameViewSize(string name, int width, int height);

        void SetGameViewSize(int index);
        (float scale, float minScale, float maxScale) GetGameViewScale();
        float SetGameViewScale(float scale);
    }
}
