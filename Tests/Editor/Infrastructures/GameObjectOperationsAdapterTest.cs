using System;
using NUnit.Framework;
using UniCortex.Editor.Infrastructures;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class GameObjectOperationsAdapterTest
    {
        private GameObjectOperationsAdapter _adapter;
        private GameObject _root;
        private GameObject _childA;
        private GameObject _childB;
        private GameObject _childC;

        [SetUp]
        public void SetUp()
        {
            _adapter = new GameObjectOperationsAdapter();
            _root = new GameObject("GameObjectOperationsAdapterTest");
            _childA = CreateChild("A", _root);
            _childB = CreateChild("B", _root);
            _childC = CreateChild("C", _root);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_root);
        }

        private static GameObject CreateChild(string name, GameObject parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static GameObject Find(int instanceId)
        {
            return (GameObject)EditorUtility.InstanceIDToObject(instanceId);
        }

        [Test]
        public void Create_WithParentAndSiblingIndex_PlacesObjectAtIndex()
        {
            // Act
            var response = _adapter.Create("New", _root.GetInstanceID(), 1, false);

            // Assert
            var go = Find(response.instanceId);
            Assert.AreSame(_root.transform, go.transform.parent);
            Assert.AreEqual(1, go.transform.GetSiblingIndex());
            Assert.AreEqual(0, _childA.transform.GetSiblingIndex());
            Assert.AreEqual(2, _childB.transform.GetSiblingIndex());
            Assert.IsNotInstanceOf<RectTransform>(go.transform);
        }

        [Test]
        public void Create_WithParentOnly_AppendsAsLastChild()
        {
            // Act
            var response = _adapter.Create("New", _root.GetInstanceID(), null, false);

            // Assert
            Assert.AreEqual(3, Find(response.instanceId).transform.GetSiblingIndex());
        }

        [Test]
        public void Create_WithSiblingIndexBeyondCount_AppendsAsLastChild()
        {
            // Act
            var response = _adapter.Create("New", _root.GetInstanceID(), 100, false);

            // Assert
            Assert.AreEqual(3, Find(response.instanceId).transform.GetSiblingIndex());
        }

        [Test]
        public void Create_WithMissingParent_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _adapter.Create("New", -999999, null, false));
        }

        [Test]
        public void Create_WithRectTransform_UsesRectTransform()
        {
            // Act
            var response = _adapter.Create("New", _root.GetInstanceID(), null, true);

            // Assert
            Assert.IsInstanceOf<RectTransform>(Find(response.instanceId).transform);
        }

        [Test]
        public void Create_UnderRectTransformParent_UsesRectTransformAndParentLayer()
        {
            // Arrange
            var canvas = new GameObject("Canvas", typeof(RectTransform)) { layer = 5 };
            canvas.transform.SetParent(_root.transform, false);

            // Act
            var response = _adapter.Create("New", canvas.GetInstanceID(), null, false);

            // Assert
            var go = Find(response.instanceId);
            Assert.IsInstanceOf<RectTransform>(go.transform);
            Assert.AreEqual(5, go.layer);
        }


        [Test]
        public void Modify_WithSiblingIndexOnly_ReordersWithinSameParent()
        {
            // Act
            _adapter.Modify(_childC.GetInstanceID(), null, null, null, null, null, 0, true);

            // Assert
            Assert.AreSame(_root.transform, _childC.transform.parent);
            Assert.AreEqual(0, _childC.transform.GetSiblingIndex());
            Assert.AreEqual(1, _childA.transform.GetSiblingIndex());
            Assert.AreEqual(2, _childB.transform.GetSiblingIndex());
        }

        [Test]
        public void Modify_WithParentAndSiblingIndex_ReparentsAtIndex()
        {
            // Arrange
            var mover = CreateChild("Mover", _childA);

            // Act
            _adapter.Modify(mover.GetInstanceID(), null, null, null, null, _root.GetInstanceID(), 1, true);

            // Assert
            Assert.AreSame(_root.transform, mover.transform.parent);
            Assert.AreEqual(1, mover.transform.GetSiblingIndex());
        }

        [Test]
        public void Modify_WithWorldPositionStaysFalse_KeepsLocalPosition()
        {
            // Arrange
            _childA.transform.localPosition = new Vector3(10f, 0f, 0f);
            _childB.transform.localPosition = new Vector3(1f, 2f, 3f);

            // Act
            _adapter.Modify(_childB.GetInstanceID(), null, null, null, null, _childA.GetInstanceID(), null, false);

            // Assert
            Assert.AreEqual(new Vector3(1f, 2f, 3f), _childB.transform.localPosition);
        }

        [Test]
        public void Modify_WithWorldPositionStaysTrue_KeepsWorldPosition()
        {
            // Arrange
            _childA.transform.localPosition = new Vector3(10f, 0f, 0f);
            _childB.transform.localPosition = new Vector3(1f, 2f, 3f);

            // Act
            _adapter.Modify(_childB.GetInstanceID(), null, null, null, null, _childA.GetInstanceID(), null, true);

            // Assert
            Assert.AreEqual(new Vector3(1f, 2f, 3f), _childB.transform.position);
        }
    }
}
