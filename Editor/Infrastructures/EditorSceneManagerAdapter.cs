using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
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
            var nodes = new List<GameObjectData>();

            foreach (var go in rootObjects)
            {
                nodes.Add(GameObjectDataBuilder.BuildNode(go.transform));
            }

            return new SceneHierarchyResponse(scene.name, scene.path, nodes);
        }
    }
}
