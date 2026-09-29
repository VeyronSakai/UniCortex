using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniCortex.Editor.Infrastructures
{
    internal static class LoadedScenes
    {
        // Returns every loaded scene in Hierarchy window order.
        // In Play Mode the DontDestroyOnLoad scene is appended when it has any root objects.
        public static List<Scene> Get()
        {
            var scenes = new List<Scene>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.IsValid() && scene.isLoaded)
                {
                    scenes.Add(scene);
                }
            }

            if (EditorApplication.isPlaying)
            {
                var dontDestroyOnLoadScene = GetDontDestroyOnLoadScene();
                if (dontDestroyOnLoadScene.IsValid() && dontDestroyOnLoadScene.rootCount > 0)
                {
                    scenes.Add(dontDestroyOnLoadScene);
                }
            }

            return scenes;
        }

        // EditorSceneManager.GetDontDestroyOnLoadScene() is internal, so reach the scene
        // through a temporary object moved into it.
        private static Scene GetDontDestroyOnLoadScene()
        {
            var probe = new GameObject("UniCortexDontDestroyOnLoadProbe") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                Object.DontDestroyOnLoad(probe);
                return probe.scene;
            }
            finally
            {
                Object.DestroyImmediate(probe);
            }
        }
    }
}
