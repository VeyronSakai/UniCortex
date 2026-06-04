using System;
using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class GameObjectOperationsAdapter : IGameObjectOperations
    {
        public List<GameObjectSearchResult> Get(string query)
        {
            using var context = SearchService.CreateContext("scene", query);
            var items = SearchService.GetItems(context, SearchFlags.Synchronous);

            var results = new List<GameObjectSearchResult>(items.Count);
            foreach (var item in items)
            {
                var go = item.ToObject<GameObject>();
                if (go == null) continue;
                results.Add(BuildSearchResult(go));
            }

            return results;
        }

        private static GameObjectSearchResult BuildSearchResult(GameObject go)
        {
            var components = go.GetComponents<Component>()
                .Where(c => c != null)
                .Select(c => c.GetType().FullName)
                .ToList();

            return new GameObjectSearchResult(
                go.name,
                go.GetInstanceID(),
                go.activeSelf,
                go.tag,
                go.layer,
                go.isStatic,
                (int)go.hideFlags,
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

        public DuplicateGameObjectResponse Duplicate(int instanceId, string name)
        {
            var source = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (source == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            // Deep copy including children and components, placed under the same parent.
            var parent = source.transform.parent;
            var clone = UnityEngine.Object.Instantiate(source, parent);

            // Use the supplied name, or a Unity-style unique sibling name (e.g. "Foo (1)").
            clone.name = string.IsNullOrEmpty(name)
                ? GameObjectUtility.GetUniqueNameForSibling(parent, source.name)
                : name;

            // Place the clone right after the source to mirror Editor "Duplicate" behavior.
            clone.transform.SetSiblingIndex(source.transform.GetSiblingIndex() + 1);

            Undo.RegisterCreatedObjectUndo(clone, "Duplicate GameObject");
            return new DuplicateGameObjectResponse(clone.name, clone.GetInstanceID());
        }
    }
}
