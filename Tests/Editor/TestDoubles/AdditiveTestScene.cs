using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace UniCortex.Editor.Tests.TestDoubles
{
    // Opens an empty scene asset additively while keeping the current active scene.
    // EditorSceneManager.NewScene(Additive) cannot be used because the Test Runner runs in an unsaved untitled scene.
    internal static class AdditiveTestScene
    {
        private const string EmptySceneYaml = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n";

        public static Scene Open(string scenePath)
        {
            File.WriteAllText(scenePath, EmptySceneYaml);
            AssetDatabase.ImportAsset(scenePath, ImportAssetOptions.ForceSynchronousImport);

            var activeScene = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(activeScene);
            return scene;
        }

        public static void Close(Scene scene, string scenePath)
        {
            EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.DeleteAsset(scenePath);
        }
    }
}
