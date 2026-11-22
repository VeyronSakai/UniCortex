using System.Collections.Generic;
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
        public int GetHierarchyCallCount { get; private set; }
        public GetHierarchyResponse HierarchyResult { get; set; }
        public List<string> DirtyScenePaths { get; set; } = new List<string>();
        public string DirtyPrefabStagePath { get; set; }
        public int DiscardChangesCallCount { get; private set; }
        public int SaveActiveSceneAsCallCount { get; private set; }
        public string LastSaveAsScenePath { get; private set; }

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

        public GetHierarchyResponse GetHierarchy()
        {
            GetHierarchyCallCount++;
            return HierarchyResult;
        }

        public IReadOnlyList<string> GetDirtyScenePaths()
        {
            return DirtyScenePaths;
        }

        public string GetDirtyPrefabStagePath()
        {
            return DirtyPrefabStagePath;
        }

        public void DiscardChanges()
        {
            DiscardChangesCallCount++;
        }

        public void SaveActiveSceneAs(string scenePath)
        {
            SaveActiveSceneAsCallCount++;
            LastSaveAsScenePath = scenePath;
        }
    }
}
