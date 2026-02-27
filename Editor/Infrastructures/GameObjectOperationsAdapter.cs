using System;
using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class GameObjectOperationsAdapter : IGameObjectOperations
    {
        public List<GameObjectBasicInfo> Find(string name, string tag, string componentType)
        {
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            var all = new List<GameObject>();
            foreach (var root in rootObjects)
            {
                CollectAll(root.transform, all);
            }

            IEnumerable<GameObject> filtered = all;

            if (!string.IsNullOrEmpty(name))
            {
                filtered = filtered.Where(go => go.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrEmpty(tag))
            {
                filtered = filtered.Where(go => go.CompareTag(tag));
            }

            if (!string.IsNullOrEmpty(componentType))
            {
                filtered = filtered.Where(go =>
                    go.GetComponents<Component>()
                        .Where(c => c != null)
                        .Any(c => c.GetType().Name == componentType));
            }

            return filtered
                .Select(go => new GameObjectBasicInfo(go.name, go.GetInstanceID(), go.activeSelf))
                .ToList();
        }

        public CreateGameObjectResponse Create(string name, string primitive)
        {
            GameObject go;
            if (!string.IsNullOrEmpty(primitive) && Enum.TryParse<PrimitiveType>(primitive, true, out var pt))
            {
                go = GameObject.CreatePrimitive(pt);
                go.name = name;
            }
            else
            {
                go = new GameObject(name);
            }

            Undo.RegisterCreatedObjectUndo(go, "Create GameObject");
            return new CreateGameObjectResponse(go.name, go.GetInstanceID());
        }

        public void Delete(int instanceId)
        {
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            Undo.DestroyObjectImmediate(go);
        }

        public GameObjectInfoResponse GetInfo(int instanceId)
        {
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            var components = go.GetComponents<Component>()
                .Where(c => c != null)
                .Select((c, i) => new ComponentInfoEntry(c.GetType().Name, i))
                .ToList();

            return new GameObjectInfoResponse(go.name, go.GetInstanceID(), go.activeSelf, go.tag, go.layer,
                components);
        }

        public void Modify(int instanceId, string name, bool? activeSelf, string tag, int? layer,
            int? parentInstanceId)
        {
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            Undo.RecordObject(go, "Modify GameObject");

            if (name != null)
            {
                go.name = name;
            }

            if (activeSelf.HasValue)
            {
                go.SetActive(activeSelf.Value);
            }

            if (tag != null)
            {
                go.tag = tag;
            }

            if (layer.HasValue)
            {
                go.layer = layer.Value;
            }

            if (parentInstanceId.HasValue)
            {
                if (parentInstanceId.Value == 0)
                {
                    Undo.SetTransformParent(go.transform, null, "Modify GameObject Parent");
                }
                else
                {
                    var parent = EditorUtility.InstanceIDToObject(parentInstanceId.Value) as GameObject;
                    if (parent != null)
                    {
                        Undo.SetTransformParent(go.transform, parent.transform, "Modify GameObject Parent");
                    }
                }
            }
        }

        private static void CollectAll(Transform transform, List<GameObject> result)
        {
            result.Add(transform.gameObject);
            for (var i = 0; i < transform.childCount; i++)
            {
                CollectAll(transform.GetChild(i), result);
            }
        }
    }
}
