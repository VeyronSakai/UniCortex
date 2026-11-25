using System.IO;
using NUnit.Framework;
using UniCortex.Editor.Infrastructures;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Tests.Infrastructures
{
    // Prefab Mode cases are covered by the Core tests: opening and closing Prefab Mode within one frame here makes
    // Unity log errors from its delayed framing of the stage.
    [TestFixture]
    internal sealed class PrefabOperationsAdapterCreateTest
    {
        private const string BasePrefabPath = "Assets/PrefabOperationsAdapterCreateTest_Base.prefab";
        private const string NewPrefabPath = "Assets/PrefabOperationsAdapterCreateTest_New.prefab";

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
            _root = new GameObject("PrefabOperationsAdapterCreateTest");
        }

        [TearDown]
        public void TearDown()
        {
            Undo.RevertAllDownToGroup(_undoGroup);
            Object.DestroyImmediate(_root);
            // Delete the Prefab that may be a Variant of the base first, so that it is not reimported without it.
            AssetDatabase.DeleteAsset(NewPrefabPath);
            AssetDatabase.DeleteAsset(BasePrefabPath);
        }

        private GameObject CreateChild(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root.transform, false);
            return go;
        }

        [Test]
        public void CreatePrefab_WithConnect_TurnsSourceIntoInstance_AndKeepsReferencesToIt()
        {
            // Arrange
            var source = CreateChild("Source");
            var body = source.AddComponent<Rigidbody>();
            var joint = CreateChild("Other").AddComponent<FixedJoint>();
            joint.connectedBody = body;

            // Act
            var response = _adapter.CreatePrefab(source.GetInstanceID(), NewPrefabPath, true);

            // Assert
            Assert.IsTrue(PrefabUtility.IsOutermostPrefabInstanceRoot(source));
            Assert.AreEqual(NewPrefabPath, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(source));
            Assert.AreSame(body, source.GetComponent<Rigidbody>());
            Assert.AreSame(body, joint.connectedBody);
            Assert.AreEqual(AssetDatabase.AssetPathToGUID(NewPrefabPath), response.guid);
        }

        [Test]
        public void CreatePrefab_WithoutConnect_LeavesSourceUnlinked()
        {
            // Arrange
            var source = CreateChild("Source");

            // Act
            var response = _adapter.CreatePrefab(source.GetInstanceID(), NewPrefabPath, false);

            // Assert
            Assert.IsFalse(PrefabUtility.IsPartOfPrefabInstance(source));
            Assert.IsTrue(File.Exists(NewPrefabPath));
            Assert.AreEqual(AssetDatabase.AssetPathToGUID(NewPrefabPath), response.guid);
        }

        [Test]
        public void CreatePrefab_WithConnect_UndoUnlinksSource_AndKeepsAsset()
        {
            // Arrange
            var source = CreateChild("Source");
            Undo.IncrementCurrentGroup();
            _adapter.CreatePrefab(source.GetInstanceID(), NewPrefabPath, true);

            // Act
            Undo.PerformUndo();

            // Assert
            Assert.IsFalse(PrefabUtility.IsPartOfPrefabInstance(source));
            Assert.IsTrue(File.Exists(NewPrefabPath));
        }

        [Test]
        public void CreatePrefab_WithConnect_PrefabInstanceRoot_BecomesInstanceOfNewVariant()
        {
            // Arrange
            var temp = new GameObject("Base");
            var baseAsset = PrefabUtility.SaveAsPrefabAsset(temp, BasePrefabPath);
            Object.DestroyImmediate(temp);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(baseAsset, _root.transform);

            // Act
            _adapter.CreatePrefab(instance.GetInstanceID(), NewPrefabPath, true);

            // Assert
            Assert.AreEqual(NewPrefabPath, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instance));
            var newAsset = AssetDatabase.LoadAssetAtPath<GameObject>(NewPrefabPath);
            Assert.AreEqual(PrefabAssetType.Variant, PrefabUtility.GetPrefabAssetType(newAsset));
        }
    }
}
