using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IEditorSceneManager
    {
        void CreateScene(string scenePath);
        void OpenScene(string scenePath);
        bool SaveOpenScenes();
        SceneHierarchyResponse GetHierarchy();
    }
}
