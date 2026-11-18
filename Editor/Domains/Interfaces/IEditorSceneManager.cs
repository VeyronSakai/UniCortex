using System.Collections.Generic;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IEditorSceneManager
    {
        bool CreateScene(string scenePath);
        void OpenScene(string scenePath);
        GetHierarchyResponse GetHierarchy();

        // Returns the paths of the loaded scenes that have unsaved changes (an empty string for an untitled scene).
        IReadOnlyList<string> GetDirtyScenePaths();

        // Drops the unsaved changes of every loaded scene: scenes with a path are reopened from disk and
        // untitled scenes are closed.
        void DiscardChanges();
    }
}
