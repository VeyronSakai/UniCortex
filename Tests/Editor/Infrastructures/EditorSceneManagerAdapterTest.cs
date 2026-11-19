using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class EditorSceneManagerAdapterTest
    {
        private const string AdditiveScenePath = "Assets/EditorSceneManagerAdapterTest.unity";
        private const string SavedAsScenePath = "Assets/EditorSceneManagerAdapterTest_SavedAs.unity";
        private const string ExistingScenePath = "Assets/EditorSceneManagerAdapterTest_Existing.unity";

        private Scene _previousActiveScene;
        private Scene _additiveScene;

        [SetUp]
        public void SetUp()
        {
            _previousActiveScene = SceneManager.GetActiveScene();
            _additiveScene = AdditiveTestScene.Open(AdditiveScenePath);
        }

        [TearDown]
        public void TearDown()
        {
            SceneManager.SetActiveScene(_previousActiveScene);
            AdditiveTestScene.Close(_additiveScene, AdditiveScenePath);
            AssetDatabase.DeleteAsset(SavedAsScenePath);
            AssetDatabase.DeleteAsset(ExistingScenePath);
        }

        [Test]
        public void GetHierarchy_WithAdditiveScene_ReturnsEveryLoadedScene()
        {
            // Arrange
            var go = new GameObject("EditorSceneManagerAdapterTest_Additive");
            SceneManager.MoveGameObjectToScene(go, _additiveScene);
            var adapter = new EditorSceneManagerAdapter();

            // Act
            var response = adapter.GetHierarchy();

            // Assert
            Assert.AreEqual(SceneManager.sceneCount, response.scenes.Count);

            var active = response.scenes.Single(s => s.isActive);
            Assert.AreEqual(SceneManager.GetActiveScene().name, active.sceneName);

            var additive = response.scenes.Last();
            Assert.IsFalse(additive.isActive);
            Assert.AreEqual("EditorSceneManagerAdapterTest", additive.sceneName);
            Assert.AreEqual(AdditiveScenePath, additive.scenePath);
            Assert.AreEqual(1, additive.gameObjects.Count);
            Assert.AreEqual("EditorSceneManagerAdapterTest_Additive", additive.gameObjects[0].name);
            Assert.AreEqual(go.GetInstanceID(), additive.gameObjects[0].instanceId);
        }

        [Test]
        public void GetDirtyScenePaths_WithDirtyAdditiveScene_ReturnsItsPath()
        {
            // Arrange
            var adapter = new EditorSceneManagerAdapter();
            Assert.That(adapter.GetDirtyScenePaths(), Does.Not.Contain(AdditiveScenePath));
            EditorSceneManager.MarkSceneDirty(_additiveScene);

            // Act
            var paths = adapter.GetDirtyScenePaths();

            // Assert
            Assert.That(paths, Does.Contain(AdditiveScenePath));
        }

        [Test]
        public void DiscardChanges_WithDirtyAdditiveScene_ReloadsItFromDisk()
        {
            // Arrange
            var go = new GameObject("EditorSceneManagerAdapterTest_Discarded");
            SceneManager.MoveGameObjectToScene(go, _additiveScene);
            EditorSceneManager.MarkSceneDirty(_additiveScene);
            var adapter = new EditorSceneManagerAdapter();

            // Act
            adapter.DiscardChanges();

            // Assert
            _additiveScene = SceneManager.GetSceneByPath(AdditiveScenePath);
            Assert.IsTrue(_additiveScene.isLoaded);
            Assert.IsFalse(_additiveScene.isDirty);
            Assert.AreEqual(0, _additiveScene.rootCount);
        }

        [Test]
        public void SaveActiveSceneAs_SavesActiveSceneToNewPath()
        {
            // Arrange
            var go = new GameObject("EditorSceneManagerAdapterTest_SavedAs");
            SceneManager.MoveGameObjectToScene(go, _additiveScene);
            SceneManager.SetActiveScene(_additiveScene);
            var adapter = new EditorSceneManagerAdapter();

            // Act
            adapter.SaveActiveSceneAs(SavedAsScenePath);

            // Assert
            Assert.AreEqual(SavedAsScenePath, _additiveScene.path);
            Assert.IsFalse(_additiveScene.isDirty);
            StringAssert.Contains("EditorSceneManagerAdapterTest_SavedAs", File.ReadAllText(SavedAsScenePath));
            StringAssert.DoesNotContain("EditorSceneManagerAdapterTest_SavedAs", File.ReadAllText(AdditiveScenePath));
        }

        [Test]
        public void SaveActiveSceneAs_ThrowsArgumentException_WhenPathAlreadyExists()
        {
            // Arrange
            AssetDatabase.CopyAsset(AdditiveScenePath, ExistingScenePath);
            var existingContent = File.ReadAllText(ExistingScenePath);
            var go = new GameObject("EditorSceneManagerAdapterTest_NotSaved");
            SceneManager.MoveGameObjectToScene(go, _additiveScene);
            SceneManager.SetActiveScene(_additiveScene);
            var adapter = new EditorSceneManagerAdapter();

            // Act
            var ex = Assert.Throws<ArgumentException>(() => adapter.SaveActiveSceneAs(ExistingScenePath));

            // Assert
            StringAssert.Contains("already exists", ex.Message);
            Assert.AreEqual(existingContent, File.ReadAllText(ExistingScenePath));
            Assert.AreEqual(AdditiveScenePath, _additiveScene.path);
        }
    }
}
