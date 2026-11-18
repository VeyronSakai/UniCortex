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

        // Reloads every loaded scene that has a path and unsaved changes from disk, dropping the changes.
        // Untitled scenes are left as they are, since they have no saved state to go back to.
        void DiscardChanges();
    }
}
