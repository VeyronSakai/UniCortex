using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class EditorSceneManagerAdapter : IEditorSceneManager
    {
        // EditorSceneManager.ReloadScene(Scene) is internal, so call it through reflection. It is what the
        // "Discard changes" item of the Hierarchy window's scene context menu calls (after its confirmation dialog).
        private static readonly MethodInfo s_reloadScene = typeof(UnityEditor.SceneManagement.EditorSceneManager)
            .GetMethod("ReloadScene", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(Scene) },
                null);

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
            // Collect the scenes first: reloading a scene may change the loaded scene list.
            var dirtyScenes = new List<Scene>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty && !string.IsNullOrEmpty(scene.path))
                {
                    dirtyScenes.Add(scene);
                }
            }

            if (dirtyScenes.Count == 0)
            {
                return;
            }

            if (s_reloadScene == null)
            {
                throw new System.InvalidOperationException(
                    "Discarding scene changes is not supported in this Unity version " +
                    "(EditorSceneManager.ReloadScene was not found).");
            }

            foreach (var scene in dirtyScenes)
            {
                if (!(bool)s_reloadScene.Invoke(null, new object[] { scene }))
                {
                    throw new System.InvalidOperationException($"Failed to reload scene '{scene.path}'.");
                }
            }
        }

        public void SaveActiveSceneAs(string scenePath)
        {
            var scene = SceneManager.GetActiveScene();

            // SaveScene overwrites the file without asking, while the save panel would ask first. Refuse instead,
            // so that another scene (possibly loaded) or asset is not replaced. Saving to its own path is a plain save.
            if (scenePath != scene.path && (File.Exists(scenePath) || Directory.Exists(scenePath)))
            {
                throw new System.ArgumentException(
                    $"'{scenePath}' already exists. Choose a path that does not exist yet.");
            }

            AssetUtility.EnsureParentFolderExists(scenePath);
            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new System.ArgumentException($"Failed to save scene at '{scenePath}'.");
            }
        }
    }
}
