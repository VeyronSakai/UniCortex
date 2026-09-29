using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace UniCortex.Editor.Infrastructures
{
    internal static class LoadedScenes
    {
        // EditorSceneManager.GetDontDestroyOnLoadScene() is internal, so call it through reflection.
        // If it is missing in a future Unity version, the DontDestroyOnLoad scene is simply skipped.
        private static readonly MethodInfo s_getDontDestroyOnLoadScene = typeof(EditorSceneManager).GetMethod(
            "GetDontDestroyOnLoadScene", BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);

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

        private static Scene GetDontDestroyOnLoadScene()
        {
            return s_getDontDestroyOnLoadScene?.Invoke(null, null) is Scene scene ? scene : default;
        }
    }
}
