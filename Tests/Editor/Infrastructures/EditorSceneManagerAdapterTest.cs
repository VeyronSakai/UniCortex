using System.Linq;
using NUnit.Framework;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class EditorSceneManagerAdapterTest
    {
        private const string AdditiveScenePath = "Assets/EditorSceneManagerAdapterTest.unity";

        private Scene _additiveScene;

        [SetUp]
        public void SetUp()
        {
            _additiveScene = AdditiveTestScene.Open(AdditiveScenePath);
        }

        [TearDown]
        public void TearDown()
        {
            AdditiveTestScene.Close(_additiveScene, AdditiveScenePath);
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
    }
}
