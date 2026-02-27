using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class EditorSceneManagerAdapter : IEditorSceneManager
    {
        public void OpenScene(string scenePath)
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
        }

        public bool SaveOpenScenes()
        {
            return UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }

        public SceneHierarchyResponse GetHierarchy()
        {
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            var nodes = new List<GameObjectNode>();

            foreach (var go in rootObjects)
            {
                nodes.Add(BuildNode(go.transform));
            }

            return new SceneHierarchyResponse(scene.name, scene.path, nodes);
        }

        private static GameObjectNode BuildNode(Transform transform)
        {
            var go = transform.gameObject;
            var components = go.GetComponents<Component>()
                .Where(c => c != null)
                .Select(c => c.GetType().Name)
                .ToList();

            var children = new List<GameObjectNode>();
            for (var i = 0; i < transform.childCount; i++)
            {
                children.Add(BuildNode(transform.GetChild(i)));
            }

            return new GameObjectNode(go.name, go.GetInstanceID(), go.activeSelf, components, children);
        }
    }
}
