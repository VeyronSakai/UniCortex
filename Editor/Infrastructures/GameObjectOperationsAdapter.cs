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
        public List<GameObjectData> Get(string query)
        {
            var parsed = SceneSearchQueryParser.Parse(query);
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();

            // If searching by instanceId, return that single object directly
            if (parsed.instanceId.HasValue)
            {
                var go = EditorUtility.InstanceIDToObject(parsed.instanceId.Value) as GameObject;
                if (go == null)
                {
                    return new List<GameObjectData>();
                }

                return new List<GameObjectData> { GameObjectDataBuilder.BuildNode(go.transform) };
            }

            // Collect all GameObjects as flat list with their hierarchy paths
            var all = new List<(GameObject go, string path)>();
            foreach (var root in rootObjects)
            {
                CollectAllWithPath(root.transform, "", all);
            }

            IEnumerable<(GameObject go, string path)> filtered = all;

            // Name filter (partial match, case-insensitive)
            if (!string.IsNullOrEmpty(parsed.namePattern))
            {
                filtered = filtered.Where(item =>
                    item.go.name.IndexOf(parsed.namePattern, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Component type filter (partial match, case-insensitive)
            if (!string.IsNullOrEmpty(parsed.componentTypePattern))
            {
                filtered = filtered.Where(item =>
                    item.go.GetComponents<Component>()
                        .Where(c => c != null)
                        .Any(c => c.GetType().Name.IndexOf(parsed.componentTypePattern,
                            StringComparison.OrdinalIgnoreCase) >= 0));
            }

            // Tag exact match
            if (!string.IsNullOrEmpty(parsed.tagExact))
            {
                filtered = filtered.Where(item => item.go.CompareTag(parsed.tagExact));
            }

            // Tag partial match (case-insensitive)
            if (!string.IsNullOrEmpty(parsed.tagPartial))
            {
                filtered = filtered.Where(item =>
                    item.go.tag.IndexOf(parsed.tagPartial, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Layer filter
            if (parsed.layer.HasValue)
            {
                filtered = filtered.Where(item => item.go.layer == parsed.layer.Value);
            }

            // Path filter (partial match, case-insensitive)
            if (!string.IsNullOrEmpty(parsed.pathPattern))
            {
                filtered = filtered.Where(item =>
                    item.path.IndexOf(parsed.pathPattern, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // State filters
            foreach (var state in parsed.stateFilters)
            {
                switch (state)
                {
                    case "root":
                        filtered = filtered.Where(item => item.go.transform.parent == null);
                        break;
                    case "child":
                        filtered = filtered.Where(item => item.go.transform.parent != null);
                        break;
                    case "leaf":
                        filtered = filtered.Where(item => item.go.transform.childCount == 0);
                        break;
                    case "static":
                        filtered = filtered.Where(item => item.go.isStatic);
                        break;
                }
            }

            return filtered
                .Select(item => GameObjectDataBuilder.BuildNode(item.go.transform))
                .ToList();
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

        private static void CollectAllWithPath(Transform transform, string parentPath,
            List<(GameObject go, string path)> result)
        {
            var currentPath = string.IsNullOrEmpty(parentPath)
                ? transform.gameObject.name
                : parentPath + "/" + transform.gameObject.name;
            result.Add((transform.gameObject, currentPath));
            for (var i = 0; i < transform.childCount; i++)
            {
                CollectAllWithPath(transform.GetChild(i), currentPath, result);
            }
        }
    }
}
