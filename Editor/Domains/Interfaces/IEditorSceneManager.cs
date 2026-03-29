using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IEditorSceneManager
    {
        bool CreateScene(string scenePath);
        void OpenScene(string scenePath);
        bool SaveOpenScenes();
        GetHierarchyResponse GetHierarchy();
    }
}
