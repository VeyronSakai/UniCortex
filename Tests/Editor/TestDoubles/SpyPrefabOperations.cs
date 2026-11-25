using System;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyPrefabOperations : IPrefabOperations
    {
        public int CreatePrefabCallCount { get; private set; }
        public int LastCreateInstanceId { get; private set; }
        public string LastCreateAssetPath { get; private set; }
        public bool LastCreateConnect { get; private set; }
        public CreatePrefabResponse CreateResult { get; set; } = new CreatePrefabResponse(true, "");

        public int InstantiateCallCount { get; private set; }
        public string LastInstantiateAssetPath { get; private set; }
        public InstantiatePrefabResponse InstantiateResult { get; set; }

        public CreatePrefabResponse CreatePrefab(int instanceId, string assetPath, bool connect)
        {
            CreatePrefabCallCount++;
            LastCreateInstanceId = instanceId;
            LastCreateAssetPath = assetPath;
            LastCreateConnect = connect;
            return CreateResult;
        }

        public InstantiatePrefabResponse InstantiatePrefab(string assetPath)
        {
            InstantiateCallCount++;
            LastInstantiateAssetPath = assetPath;
            return InstantiateResult;
        }

        public int OpenPrefabCallCount { get; private set; }
        public string LastOpenAssetPath { get; private set; }

        public void OpenPrefab(string assetPath)
        {
            OpenPrefabCallCount++;
            LastOpenAssetPath = assetPath;
        }

        public int ClosePrefabCallCount { get; private set; }
        public bool LastCloseDiscardChanges { get; private set; }

        public void ClosePrefab(bool discardChanges = false)
        {
            ClosePrefabCallCount++;
            LastCloseDiscardChanges = discardChanges;
        }

        public int UnpackCallCount { get; private set; }
        public int LastUnpackInstanceId { get; private set; }
        public string LastUnpackMode { get; private set; }
        public Exception UnpackException { get; set; }

        public void UnpackPrefabInstance(int instanceId, string mode)
        {
            UnpackCallCount++;
            LastUnpackInstanceId = instanceId;
            LastUnpackMode = mode;
            if (UnpackException != null)
            {
                throw UnpackException;
            }
        }
    }
}
