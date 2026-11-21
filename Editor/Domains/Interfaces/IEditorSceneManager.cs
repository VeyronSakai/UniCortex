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

        // Returns the asset path of the Prefab open in Prefab Mode if it has unsaved changes, or null.
        // With Auto Save on, the changes are saved as they are made, so it has none.
        string GetDirtyPrefabStagePath();

        // Reloads every loaded scene that has a path and unsaved changes from disk, dropping the changes.
        // Untitled scenes are left as they are, since they have no saved state to go back to.
        void DiscardChanges();

        // Saves the active scene to the given path without the save panel, like File > Save As.
        // The scene then refers to the new path. Fails if another file already exists at the path.
        void SaveActiveSceneAs(string scenePath);
    }
}
