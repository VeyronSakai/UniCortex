using System.Linq;
using NUnit.Framework;
using UniCortex.Editor.Infrastructures;
using UnityEngine;

namespace UniCortex.Tests
{
    // The DontDestroyOnLoad scene only exists in Play Mode, so these are Play Mode tests.
    [TestFixture]
    internal sealed class DontDestroyOnLoadSceneTest
    {
        private const string DontDestroyOnLoadSceneName = "DontDestroyOnLoad";
        private const string TargetName = "DontDestroyOnLoadSceneTest_Target";

        private GameObject _target;

        [SetUp]
        public void SetUp()
        {
            _target = new GameObject(TargetName);
            Object.DontDestroyOnLoad(_target);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_target);
        }

        [Test]
        public void GetHierarchy_IncludesDontDestroyOnLoadScene()
        {
            // Arrange
            var adapter = new EditorSceneManagerAdapter();

            // Act
            var response = adapter.GetHierarchy();

            // Assert
            var scene = response.scenes.Single(s => s.sceneName == DontDestroyOnLoadSceneName);
            Assert.IsFalse(scene.isActive);
            CollectionAssert.Contains(scene.gameObjects.Select(n => n.instanceId).ToList(), _target.GetInstanceID());
        }

        [Test]
        public void Get_FindsObjectInDontDestroyOnLoadScene()
        {
            // Arrange
            var adapter = new GameObjectOperationsAdapter();

            // Act
            var results = adapter.Get(TargetName);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(_target.GetInstanceID(), results[0].instanceId);
            Assert.AreEqual(DontDestroyOnLoadSceneName, results[0].sceneName);
        }
    }
}
