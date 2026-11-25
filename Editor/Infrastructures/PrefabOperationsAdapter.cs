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
        public CreatePrefabResponse CreatePrefab(int instanceId, string assetPath, bool connect)
        {
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            // Saving over the Prefab open in Prefab Mode would replace the asset being edited with the GameObject.
            var currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (currentPrefabStage != null && string.Equals(currentPrefabStage.assetPath, assetPath.Replace('\\', '/'),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Cannot save to '{assetPath}': it is the Prefab open in Prefab Mode, which it would replace.");
            }

            // Connecting the root of the Prefab open in Prefab Mode would turn that Prefab into a Variant of the new one.
            var sourcePrefabStage = PrefabStageUtility.GetPrefabStage(go);
            if (connect && sourcePrefabStage != null && sourcePrefabStage.prefabContentsRoot == go)
            {
                throw new ArgumentException(
                    $"Cannot connect '{go.name}': it is the root of the Prefab open in Prefab Mode " +
                    $"({sourcePrefabStage.assetPath}). Connect one of its children, or save it without connect.");
            }

            AssetUtility.EnsureParentFolderExists(assetPath);

            // UserAction records the connection as one Undo step. Undo does not delete the saved asset.
            var asset = connect
                ? PrefabUtility.SaveAsPrefabAssetAndConnect(go, assetPath, InteractionMode.UserAction)
                : PrefabUtility.SaveAsPrefabAsset(go, assetPath);
            if (asset == null)
            {
                throw new ArgumentException($"Failed to save prefab at '{assetPath}'.");
            }

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            return new CreatePrefabResponse(true, guid);
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
