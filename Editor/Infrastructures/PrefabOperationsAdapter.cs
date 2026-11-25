using System;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class PrefabOperationsAdapter : IPrefabOperations
    {
        public void CreatePrefab(int instanceId, string assetPath)
        {
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            AssetUtility.EnsureParentFolderExists(assetPath);

            if (PrefabUtility.SaveAsPrefabAsset(go, assetPath) == null)
            {
                throw new ArgumentException($"Failed to save prefab at '{assetPath}'.");
            }
        }

        public InstantiatePrefabResponse InstantiatePrefab(string assetPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                throw new ArgumentException($"Prefab not found at path: {assetPath}");
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Instantiate Prefab");

            return new InstantiatePrefabResponse(instance.name, instance.GetInstanceID());
        }

        public void OpenPrefab(string assetPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                throw new ArgumentException($"Prefab not found at path: {assetPath}");
            }

            PrefabStageUtility.OpenPrefab(assetPath);
        }

        public void ClosePrefab(bool discardChanges = false)
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (discardChanges && prefabStage != null)
            {
                prefabStage.ClearDirtiness();
            }

            StageUtility.GoToMainStage();
        }

        public void UnpackPrefabInstance(int instanceId, string mode)
        {
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            if (!PrefabUtility.IsOutermostPrefabInstanceRoot(go))
            {
                var outermostRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                if (outermostRoot == null)
                {
                    throw new ArgumentException($"'{go.name}' (instanceId {instanceId}) is not a prefab instance.");
                }

                throw new ArgumentException(
                    $"'{go.name}' (instanceId {instanceId}) is not the outermost root of a prefab instance. " +
                    $"Unpack its outermost root '{outermostRoot.name}' " +
                    $"(instanceId {outermostRoot.GetInstanceID()}) instead.");
            }

            var unpackMode = mode == PrefabUnpackModes.Completely
                ? PrefabUnpackMode.Completely
                : PrefabUnpackMode.OutermostRoot;

            // UserAction records the unpack as one Undo step.
            PrefabUtility.UnpackPrefabInstance(go, unpackMode, InteractionMode.UserAction);
        }
    }
}
