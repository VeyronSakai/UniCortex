using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Search;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class GameObjectOperationsAdapter : IGameObjectOperations
    {
        private const string ModifyParentUndoName = "Modify GameObject Parent";

        // The scene provider caches its object list and only refreshes it on hierarchy events.
        // After the domain reload on entering Play Mode those events are not hooked up, so objects
        // created or loaded at runtime (e.g. additively loaded scenes) are missed. There is no public
        // API to invalidate that cache, so call the internal hook the provider listens to.
        private static readonly MethodInfo s_invalidateSearchScene = typeof(SearchMonitor).GetMethod(
            "InvalidateCurrentScene", BindingFlags.NonPublic | BindingFlags.Static);

        public List<GameObjectSearchResult> Get(string query)
        {
            var parsed = GameObjectQuery.Parse(query);
            var gameObjects = string.IsNullOrEmpty(parsed.SearchQuery)
                ? EnumerateAllGameObjects()
                : Search(parsed.SearchQuery);

            var results = new List<GameObjectSearchResult>();
            foreach (var go in gameObjects)
            {
                if (!parsed.MatchesScene(go.scene.name)) continue;
                results.Add(BuildSearchResult(go));
            }

            return results;
        }

        private static IEnumerable<GameObject> Search(string query)
        {
            s_invalidateSearchScene?.Invoke(null, null);

            using var context = SearchService.CreateContext("scene", query);
            var items = SearchService.GetItems(context, SearchFlags.Synchronous);

            var gameObjects = new List<GameObject>(items.Count);
            foreach (var item in items)
            {
                var go = item.ToObject<GameObject>();
                if (go == null) continue;
                gameObjects.Add(go);
            }

            return gameObjects;
        }

        // Used when the query only narrows by scene, since Unity Search returns nothing for an empty query.
        private static IEnumerable<GameObject> EnumerateAllGameObjects()
        {
            var roots = new List<GameObject>();
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                roots.Add(prefabStage.prefabContentsRoot);
            }
            else
            {
                foreach (var scene in LoadedScenes.Get())
                {
                    roots.AddRange(scene.GetRootGameObjects());
                }
            }

            var gameObjects = new List<GameObject>();
            foreach (var root in roots)
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    gameObjects.Add(transform.gameObject);
                }
            }

            return gameObjects;
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
                components,
                go.scene.name);
        }

        public CreateGameObjectResponse Create(string name, int parentInstanceId, int? siblingIndex,
            bool useRectTransform)
        {
            GameObject parent = null;
            if (parentInstanceId != 0)
            {
                parent = EditorUtility.InstanceIDToObject(parentInstanceId) as GameObject;
                if (parent == null)
                {
                    throw new ArgumentException($"Parent GameObject with instanceId {parentInstanceId} not found.");
                }
            }

            // Mirror "Create Empty Child": children of UI objects get a RectTransform automatically.
            var needsRectTransform = useRectTransform || (parent != null && parent.transform is RectTransform);
            var go = needsRectTransform ? new GameObject(name, typeof(RectTransform)) : new GameObject(name);

            if (parent != null)
            {
                // Resets the local transform and inherits the parent's layer, like the Editor's create commands.
                GameObjectUtility.SetParentAndAlign(go, parent);
            }

            if (siblingIndex.HasValue)
            {
                // Unity places the object last when the index exceeds the sibling count.
                go.transform.SetSiblingIndex(siblingIndex.Value);
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

        public void Modify(int instanceId, string name, bool? activeSelf, string tag, int? layer,
            int? parentInstanceId, int? siblingIndex, bool worldPositionStays)
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
                    Undo.SetTransformParent(go.transform, null, worldPositionStays, ModifyParentUndoName);
                }
                else
                {
                    var parent = EditorUtility.InstanceIDToObject(parentInstanceId.Value) as GameObject;
                    if (parent != null)
                    {
                        Undo.SetTransformParent(go.transform, parent.transform, worldPositionStays,
                            ModifyParentUndoName);
                    }
                }
            }

            if (siblingIndex.HasValue)
            {
                Undo.SetSiblingIndex(go.transform, siblingIndex.Value, "Modify GameObject Sibling Index");
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
