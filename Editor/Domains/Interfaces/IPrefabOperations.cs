using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IPrefabOperations
    {
        void CreatePrefab(int instanceId, string assetPath);
        InstantiatePrefabResponse InstantiatePrefab(string assetPath);
    }
}
