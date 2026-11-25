using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IPrefabOperations
    {
        // Saves the GameObject as a Prefab asset. With connect, the GameObject becomes an instance of the new asset.
        CreatePrefabResponse CreatePrefab(int instanceId, string assetPath, bool connect);
        InstantiatePrefabResponse InstantiatePrefab(string assetPath);
        void OpenPrefab(string assetPath);
        // Leaves Prefab Mode. With discardChanges, the unsaved changes of the Prefab are dropped first, so that
        // Unity does not ask whether to save them.
        void ClosePrefab(bool discardChanges = false);
        // Unpacks the prefab instance whose outermost root is the given GameObject, with a PrefabUnpackModes value.
        void UnpackPrefabInstance(int instanceId, string mode);
    }
}
