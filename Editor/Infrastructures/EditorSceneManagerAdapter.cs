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
            AssetUtility.EnsureParentFolderExists(scenePath);
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new System.ArgumentException($"Failed to save scene at '{scenePath}'.");
            }

            return true;
        }

        public void OpenScene(string scenePath)
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
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
                return new GetHierarchyResponse(new List<SceneHierarchy>
                {
                    new SceneHierarchy(root.name, prefabStage.assetPath, true, prefabNodes)
                });
            }

            var activeScene = SceneManager.GetActiveScene();
            var scenes = new List<SceneHierarchy>();

            foreach (var scene in LoadedScenes.Get())
            {
                var nodes = new List<GameObjectNode>();
                foreach (var go in scene.GetRootGameObjects())
                {
                    nodes.Add(GameObjectNodeBuilder.BuildNode(go.transform));
                }

                scenes.Add(new SceneHierarchy(scene.name, scene.path, scene == activeScene, nodes));
            }

            return new GetHierarchyResponse(scenes);
        }

        public IReadOnlyList<string> GetDirtyScenePaths()
        {
            var paths = new List<string>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    paths.Add(scene.path);
                }
            }

            return paths;
        }

        public void DiscardChanges()
        {
            if (GetDirtyScenePaths().Count == 0)
            {
                return;
            }

            // Reopen the scenes that have a path from disk, keeping their order, loaded state and the active scene.
            // An untitled scene has no saved state to go back to, so it is dropped.
            var setups = new List<SceneSetup>();
            foreach (var setup in UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup())
            {
                if (!string.IsNullOrEmpty(setup.path))
                {
                    setups.Add(setup);
                }
            }

            var activeIndex = setups.FindIndex(setup => setup.isActive);
            if (activeIndex < 0)
            {
                // The active scene was untitled: make the first loaded scene active instead.
                activeIndex = setups.FindIndex(setup => setup.isLoaded);
            }

            if (activeIndex < 0)
            {
                // No scene with a path is loaded, so there is nothing to reopen.
                UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                    UnityEditor.SceneManagement.NewSceneMode.Single);
                return;
            }

            setups[activeIndex].isActive = true;
            UnityEditor.SceneManagement.EditorSceneManager.RestoreSceneManagerSetup(setups.ToArray());
        }
    }
}
