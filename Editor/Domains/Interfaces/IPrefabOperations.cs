using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IPrefabOperations
    {
        void CreatePrefab(int instanceId, string assetPath);
        InstantiatePrefabResponse InstantiatePrefab(string assetPath);
        void OpenPrefab(string assetPath);
        // Leaves Prefab Mode. With discardChanges, the unsaved changes of the Prefab are dropped first, so that
        // Unity does not ask whether to save them.
        void ClosePrefab(bool discardChanges);
    }
}
