using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyEditorSceneManager : IEditorSceneManager
    {
        public int CreateSceneCallCount { get; private set; }
        public string LastCreateScenePath { get; private set; }
        public bool CreateSceneResult { get; set; } = true;
        public int OpenSceneCallCount { get; private set; }
        public string LastScenePath { get; private set; }
        public int SaveOpenScenesCallCount { get; private set; }
        public bool SaveOpenScenesResult { get; set; } = true;
        public int GetHierarchyCallCount { get; private set; }
        public GetHierarchyResponse HierarchyResult { get; set; }

        public bool CreateScene(string scenePath)
        {
            CreateSceneCallCount++;
            LastCreateScenePath = scenePath;
            return CreateSceneResult;
        }

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

        public GetHierarchyResponse GetHierarchy()
        {
            GetHierarchyCallCount++;
            return HierarchyResult;
        }
    }
}
