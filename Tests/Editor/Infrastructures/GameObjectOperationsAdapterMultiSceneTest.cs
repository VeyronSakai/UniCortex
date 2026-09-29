using System.Linq;
using NUnit.Framework;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class GameObjectOperationsAdapterMultiSceneTest
    {
        private const string AdditiveScenePath = "Assets/GameObjectOperationsAdapterMultiSceneTest.unity";
        private const string AdditiveSceneName = "GameObjectOperationsAdapterMultiSceneTest";
        private const string TargetName = "GameObjectOperationsAdapterMultiSceneTest_Target";

        private GameObjectOperationsAdapter _adapter;
        private Scene _additiveScene;
        private GameObject _activeSceneObject;
        private GameObject _additiveRoot;
        private GameObject _additiveChild;

        [SetUp]
        public void SetUp()
        {
            _adapter = new GameObjectOperationsAdapter();

            _additiveScene = AdditiveTestScene.Open(AdditiveScenePath);

            _activeSceneObject = new GameObject(TargetName);

            _additiveRoot = new GameObject(TargetName, typeof(BoxCollider));
            SceneManager.MoveGameObjectToScene(_additiveRoot, _additiveScene);
            _additiveChild = new GameObject("GameObjectOperationsAdapterMultiSceneTest_Child");
            _additiveChild.transform.SetParent(_additiveRoot.transform, false);
            _additiveChild.SetActive(false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_activeSceneObject);
            AdditiveTestScene.Close(_additiveScene, AdditiveScenePath);
        }

        [Test]
        public void Get_ByName_FindsObjectsInEveryLoadedScene()
        {
            // Act
            var results = _adapter.Get(TargetName);

            // Assert
            var ids = results.Select(r => r.instanceId).ToList();
            CollectionAssert.Contains(ids, _activeSceneObject.GetInstanceID());
            CollectionAssert.Contains(ids, _additiveRoot.GetInstanceID());

            var additive = results.Single(r => r.instanceId == _additiveRoot.GetInstanceID());
            Assert.AreEqual(AdditiveSceneName, additive.sceneName);
        }

        [Test]
        public void Get_WithSceneToken_ReturnsOnlyObjectsInThatScene()
        {
            // Act
            var results = _adapter.Get($"{TargetName} scene:{AdditiveSceneName}");

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(_additiveRoot.GetInstanceID(), results[0].instanceId);
        }

        [Test]
        public void Get_WithOnlySceneToken_ReturnsEveryObjectInThatScene()
        {
            // Act
            var results = _adapter.Get($"scene:{AdditiveSceneName}");

            // Assert
            CollectionAssert.AreEqual(
                new[] { _additiveRoot.GetInstanceID(), _additiveChild.GetInstanceID() },
                results.Select(r => r.instanceId).ToArray());
            Assert.IsTrue(results.All(r => r.sceneName == AdditiveSceneName));
        }

        [Test]
        public void Get_WithUnknownSceneToken_ReturnsEmpty()
        {
            // Act
            var results = _adapter.Get($"{TargetName} scene:NoSuchScene");

            // Assert
            Assert.AreEqual(0, results.Count);
        }
    }
}
