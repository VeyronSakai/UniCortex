using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyEditorSceneManager : IEditorSceneManager
    {
        public int OpenSceneCallCount { get; private set; }
        public string LastScenePath { get; private set; }
        public int SaveOpenScenesCallCount { get; private set; }
        public bool SaveOpenScenesResult { get; set; } = true;
        public int GetHierarchyCallCount { get; private set; }
        public SceneHierarchyResponse HierarchyResult { get; set; }

        public void OpenScene(string scenePath)
        {
            OpenSceneCallCount++;
            LastScenePath = scenePath;
        }

        public bool SaveOpenScenes()
        {
            SaveOpenScenesCallCount++;
            return SaveOpenScenesResult;
        }

        public SceneHierarchyResponse GetHierarchy()
        {
            GetHierarchyCallCount++;
            return HierarchyResult;
        }
    }
}
