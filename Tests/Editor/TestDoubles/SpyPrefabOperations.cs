using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyPrefabOperations : IPrefabOperations
    {
        public int CreatePrefabCallCount { get; private set; }
        public int LastCreateInstanceId { get; private set; }
        public string LastCreateAssetPath { get; private set; }

        public int InstantiateCallCount { get; private set; }
        public string LastInstantiateAssetPath { get; private set; }
        public InstantiatePrefabResponse InstantiateResult { get; set; }

        public void CreatePrefab(int instanceId, string assetPath)
        {
            CreatePrefabCallCount++;
            LastCreateInstanceId = instanceId;
            LastCreateAssetPath = assetPath;
        }

        public InstantiatePrefabResponse InstantiatePrefab(string assetPath)
        {
            InstantiateCallCount++;
            LastInstantiateAssetPath = assetPath;
            return InstantiateResult;
        }
    }
}
