using System;
using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class GameObjectOperationsAdapter : IGameObjectOperations
    {
        public List<GameObjectSearchResult> Get(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return GetAllGameObjects();
            }

            using var context = SearchService.CreateContext("scene", query);
            var items = SearchService.GetItems(context, SearchFlags.Synchronous);

            var results = new List<GameObjectSearchResult>();
            foreach (var item in items)
            {
                var go = item.ToObject<GameObject>();
                if (go == null) continue;
                results.Add(BuildSearchResult(go));
            }

            return results;
        }

        private static List<GameObjectSearchResult> GetAllGameObjects()
        {
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            var results = new List<GameObjectSearchResult>();
            foreach (var root in rootObjects)
            {
                CollectAll(root.transform, results);
            }

            return results;
        }

        private static void CollectAll(Transform transform, List<GameObjectSearchResult> results)
        {
            results.Add(BuildSearchResult(transform.gameObject));
            for (var i = 0; i < transform.childCount; i++)
            {
                CollectAll(transform.GetChild(i), results);
            }
        }

        private static GameObjectSearchResult BuildSearchResult(GameObject go)
        {
            var components = go.GetComponents<Component>()
                .Where(c => c != null)
                .Select(c => c.GetType().Name)
                .ToList();

            var isLocked = (go.hideFlags & HideFlags.NotEditable) != 0;
            return new GameObjectSearchResult(
                go.name,
                go.GetInstanceID(),
                go.activeSelf,
                go.tag,
                go.layer,
                go.isStatic,
                isLocked,
                components);
        }

        public CreateGameObjectResponse Create(string name)
        {
            var go = new GameObject(name);
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
    }
}
