using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEngine.SceneManagement;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class EditorSceneManagerAdapter : IEditorSceneManager
    {
        private const string TempScenePath = "Assets/_UniCortex_Temp_Scene.unity";

        public bool CreateScene(string scenePath)
        {
            SaveIfDirty();
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            var result = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
            CleanupTempScene();
            return result;
        }

        public void OpenScene(string scenePath)
        {
            SaveIfDirty();
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            CleanupTempScene();
        }

        // Save all dirty scenes to prevent "Scene(s) Have Been Modified" dialog
        // when NewScene(Single) or OpenScene(Single) closes the current scene.
        // For Untitled scenes (no path), save to a temp path to clear the dirty flag
        // without showing the "Save Scene" dialog.
        private static void SaveIfDirty()
        {
            var sceneCount = SceneManager.sceneCount;
            for (var i = 0; i < sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isDirty) continue;

                if (string.IsNullOrEmpty(scene.path))
                {
                    // Save Untitled scene to a temp path to clear the dirty flag.
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, TempScenePath);
                    continue;
                }

                if (!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene))
                {
                    throw new System.InvalidOperationException(
                        "Failed to save scene: " + scene.path);
                }
            }
        }

        private static void CleanupTempScene()
        {
            if (UnityEditor.AssetDatabase.DeleteAsset(TempScenePath))
            {
                UnityEditor.AssetDatabase.Refresh();
            }
        }

        public bool SaveOpenScenes()
        {
            var sceneCount = SceneManager.sceneCount;
            for (var i = 0; i < sceneCount; i++)
            {
                if (string.IsNullOrEmpty(SceneManager.GetSceneAt(i).path))
                {
                    throw new System.InvalidOperationException(
                        "Cannot save Untitled scene. Create or open a named scene first.");
                }
            }

            return UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }

        public SceneHierarchyResponse GetHierarchy()
        {
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            var nodes = new List<GameObjectNode>();

            foreach (var go in rootObjects)
            {
                nodes.Add(GameObjectNodeBuilder.BuildNode(go.transform));
            }

            return new SceneHierarchyResponse(scene.name, scene.path, nodes);
        }
    }
}
