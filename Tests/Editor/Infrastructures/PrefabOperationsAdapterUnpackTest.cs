using System;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class PrefabOperationsAdapterUnpackTest
    {
        private const string InnerPrefabPath = "Assets/PrefabOperationsAdapterUnpackTest_Inner.prefab";
        private const string OuterPrefabPath = "Assets/PrefabOperationsAdapterUnpackTest_Outer.prefab";

        private PrefabOperationsAdapter _adapter;
        private GameObject _root;
        private int _undoGroup;

        [SetUp]
        public void SetUp()
        {
            // Start a new Undo group so TearDown can revert everything the adapter records.
            Undo.IncrementCurrentGroup();
            _undoGroup = Undo.GetCurrentGroup();

            _adapter = new PrefabOperationsAdapter();
            _root = new GameObject("PrefabOperationsAdapterUnpackTest");
        }

        [TearDown]
        public void TearDown()
        {
            Undo.RevertAllDownToGroup(_undoGroup);
            Object.DestroyImmediate(_root);
            AssetDatabase.DeleteAsset(OuterPrefabPath);
            AssetDatabase.DeleteAsset(InnerPrefabPath);
        }

        // Creates the Outer prefab, which contains a nested instance of the Inner prefab, and returns an
        // instance of Outer under _root.
        private GameObject InstantiateOuterWithNestedInner()
        {
            var innerTemp = new GameObject("Inner");
            new GameObject("InnerChild").transform.SetParent(innerTemp.transform, false);
            var innerAsset = PrefabUtility.SaveAsPrefabAsset(innerTemp, InnerPrefabPath);
            Object.DestroyImmediate(innerTemp);

            var outerTemp = new GameObject("Outer");
            PrefabUtility.InstantiatePrefab(innerAsset, outerTemp.transform);
            var outerAsset = PrefabUtility.SaveAsPrefabAsset(outerTemp, OuterPrefabPath);
            Object.DestroyImmediate(outerTemp);

            return (GameObject)PrefabUtility.InstantiatePrefab(outerAsset, _root.transform);
        }

        [Test]
        public void UnpackPrefabInstance_OutermostRoot_KeepsNestedInstanceLinked()
        {
            // Arrange
            var outer = InstantiateOuterWithNestedInner();
            var nested = outer.transform.GetChild(0).gameObject;

            // Act
            _adapter.UnpackPrefabInstance(outer.GetInstanceID(), PrefabUnpackModes.OutermostRoot);

            // Assert
            Assert.IsFalse(PrefabUtility.IsPartOfPrefabInstance(outer));
            Assert.IsTrue(PrefabUtility.IsOutermostPrefabInstanceRoot(nested));
            Assert.AreEqual(InnerPrefabPath, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(nested));
        }

        [Test]
        public void UnpackPrefabInstance_Completely_UnpacksNestedInstanceToo()
        {
            // Arrange
            var outer = InstantiateOuterWithNestedInner();
            var nested = outer.transform.GetChild(0).gameObject;

            // Act
            _adapter.UnpackPrefabInstance(outer.GetInstanceID(), PrefabUnpackModes.Completely);

            // Assert
            Assert.IsFalse(PrefabUtility.IsPartOfPrefabInstance(outer));
            Assert.IsFalse(PrefabUtility.IsPartOfPrefabInstance(nested));
            Assert.IsFalse(PrefabUtility.IsPartOfPrefabInstance(nested.transform.GetChild(0).gameObject));
        }

        [Test]
        public void UnpackPrefabInstance_CanBeUndone()
        {
            // Arrange
            var outer = InstantiateOuterWithNestedInner();
            Undo.IncrementCurrentGroup();
            _adapter.UnpackPrefabInstance(outer.GetInstanceID(), PrefabUnpackModes.Completely);

            // Act
            Undo.PerformUndo();

            // Assert
            Assert.IsTrue(PrefabUtility.IsOutermostPrefabInstanceRoot(outer));
            Assert.AreEqual(OuterPrefabPath, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(outer));
        }

        [Test]
        public void UnpackPrefabInstance_NestedInstanceRoot_ThrowsWithOutermostRoot()
        {
            // Arrange
            var outer = InstantiateOuterWithNestedInner();
            var nested = outer.transform.GetChild(0).gameObject;

            // Act
            var ex = Assert.Throws<ArgumentException>(() =>
                _adapter.UnpackPrefabInstance(nested.GetInstanceID(), PrefabUnpackModes.OutermostRoot));

            // Assert
            StringAssert.Contains("is not the outermost root of a prefab instance", ex.Message);
            StringAssert.Contains($"(instanceId {outer.GetInstanceID()})", ex.Message);
            Assert.IsTrue(PrefabUtility.IsOutermostPrefabInstanceRoot(outer));
        }

        [Test]
        public void UnpackPrefabInstance_NotPrefabInstance_Throws()
        {
            // Arrange
            var go = new GameObject("Plain");
            go.transform.SetParent(_root.transform, false);

            // Act
            var ex = Assert.Throws<ArgumentException>(() =>
                _adapter.UnpackPrefabInstance(go.GetInstanceID(), PrefabUnpackModes.OutermostRoot));

            // Assert
            StringAssert.Contains("is not a prefab instance", ex.Message);
        }

        [Test]
        public void UnpackPrefabInstance_MissingGameObject_Throws()
        {
            // Act
            var ex = Assert.Throws<ArgumentException>(() =>
                _adapter.UnpackPrefabInstance(int.MaxValue, PrefabUnpackModes.OutermostRoot));

            // Assert
            StringAssert.Contains("not found", ex.Message);
        }
    }
}
