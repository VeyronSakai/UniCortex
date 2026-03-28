using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class EditorSceneManagerAdapter : IEditorSceneManager
    {
        public bool CreateScene(string scenePath)
        {
            SaveIfDirty();
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            return UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
        }

        public void OpenScene(string scenePath)
        {
            SaveIfDirty();
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
        }

        // Save all dirty scenes to prevent "Scene(s) Have Been Modified" dialog
        // when NewScene(Single) or OpenScene(Single) closes the current scene.
        private static void SaveIfDirty()
        {
            var sceneCount = SceneManager.sceneCount;
            for (var i = 0; i < sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).isDirty)
                {
                    if (!UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes())
                    {
                        throw new System.InvalidOperationException(
                            "Failed to save open scenes before scene operation.");
                    }

                    return;
                }
            }
        }

        public bool SaveOpenScenes()
        {
            return UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }

        public GetHierarchyResponse GetHierarchy()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                var root = prefabStage.prefabContentsRoot;
                var prefabNodes = new List<GameObjectNode>
                {
                    GameObjectNodeBuilder.BuildNode(root.transform)
                };
                return new GetHierarchyResponse(root.name, prefabStage.assetPath, prefabNodes);
            }

            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            var nodes = new List<GameObjectNode>();

            foreach (var go in rootObjects)
            {
                nodes.Add(GameObjectNodeBuilder.BuildNode(go.transform));
            }

            return new GetHierarchyResponse(scene.name, scene.path, nodes);
        }
    }
}
