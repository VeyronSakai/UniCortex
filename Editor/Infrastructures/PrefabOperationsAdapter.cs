using System;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
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

            PrefabUtility.SaveAsPrefabAsset(go, assetPath);
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
    }
}
