using NUnit.Framework;
using UniCortex.Editor.Infrastructures;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class GameObjectOperationsAdapterDuplicateTest
    {
        private const string InnerPrefabPath = "Assets/GameObjectOperationsAdapterDuplicateTest_Inner.prefab";
        private const string OuterPrefabPath = "Assets/GameObjectOperationsAdapterDuplicateTest_Outer.prefab";

        private GameObjectOperationsAdapter _adapter;
        private GameObject _root;
        private int _undoGroup;

        [SetUp]
        public void SetUp()
        {
            // Start a new Undo group so TearDown can revert everything the adapter records.
            Undo.IncrementCurrentGroup();
            _undoGroup = Undo.GetCurrentGroup();

            _adapter = new GameObjectOperationsAdapter();
            _root = new GameObject("GameObjectOperationsAdapterDuplicateTest");
        }

        [TearDown]
        public void TearDown()
        {
            Undo.RevertAllDownToGroup(_undoGroup);
            Object.DestroyImmediate(_root);
            AssetDatabase.DeleteAsset(OuterPrefabPath);
            AssetDatabase.DeleteAsset(InnerPrefabPath);
        }

        private GameObject CreateChild(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root.transform, false);
            return go;
        }

        private static GameObject Find(int instanceId)
        {
            return (GameObject)EditorUtility.InstanceIDToObject(instanceId);
        }

        private static GameObject CreateInnerPrefab()
        {
            var temp = new GameObject("Inner");
            new GameObject("InnerChild").transform.SetParent(temp.transform, false);
            var asset = PrefabUtility.SaveAsPrefabAsset(temp, InnerPrefabPath);
            Object.DestroyImmediate(temp);
            return asset;
        }

        [Test]
        public void Duplicate_PlacesCloneRightAfterSourceWithUniqueName()
        {
            // Arrange
            var source = CreateChild("Source");
            CreateChild("Other");

            // Act
            var response = _adapter.Duplicate(source.GetInstanceID(), null);

            // Assert
            var clone = Find(response.instanceId);
            Assert.AreNotSame(source, clone);
            Assert.AreSame(_root.transform, clone.transform.parent);
            Assert.AreEqual(1, clone.transform.GetSiblingIndex());
            Assert.AreEqual("Source (1)", response.name);
        }

        [Test]
        public void Duplicate_CopiesChildrenAndComponents()
        {
            // Arrange
            var source = CreateChild("Source");
            source.AddComponent<BoxCollider>();
            new GameObject("Child").transform.SetParent(source.transform, false);

            // Act
            var response = _adapter.Duplicate(source.GetInstanceID(), null);

            // Assert
            var clone = Find(response.instanceId);
            Assert.IsNotNull(clone.GetComponent<BoxCollider>());
            Assert.AreEqual(1, clone.transform.childCount);
            Assert.AreEqual("Child", clone.transform.GetChild(0).name);
        }

        [Test]
        public void Duplicate_WithName_UsesProvidedName()
        {
            // Arrange
            var source = CreateChild("Source");

            // Act
            var response = _adapter.Duplicate(source.GetInstanceID(), "Custom");

            // Assert
            Assert.AreEqual("Custom", response.name);
            Assert.AreEqual("Custom", Find(response.instanceId).name);
        }

        [Test]
        public void Duplicate_RestoresPreviousSelection()
        {
            // Arrange
            var source = CreateChild("Source");
            var selected = CreateChild("Selected");
            Selection.objects = new Object[] { selected };

            // Act
            _adapter.Duplicate(source.GetInstanceID(), null);

            // Assert
            Assert.AreEqual(1, Selection.objects.Length);
            Assert.AreSame(selected, Selection.activeGameObject);
        }

        [Test]
        public void Duplicate_KeepsSystemClipboard()
        {
            // Arrange
            var source = CreateChild("Source");
            var previousClipboard = EditorGUIUtility.systemCopyBuffer;
            EditorGUIUtility.systemCopyBuffer = "clipboard text";

            try
            {
                // Act
                _adapter.Duplicate(source.GetInstanceID(), null);

                // Assert
                Assert.AreEqual("clipboard text", EditorGUIUtility.systemCopyBuffer);
            }
            finally
            {
                EditorGUIUtility.systemCopyBuffer = previousClipboard;
            }
        }

        [Test]
        public void Duplicate_SingleUndo_RemovesCloneAndName()
        {
            // Arrange
            var source = CreateChild("Source");
            var response = _adapter.Duplicate(source.GetInstanceID(), "Custom");

            // Act
            Undo.PerformUndo();

            // Assert
            Assert.IsTrue(Find(response.instanceId) == null);
            Assert.AreEqual(1, _root.transform.childCount);
        }

        [Test]
        public void Duplicate_PrefabInstance_KeepsPrefabLinkAndOverrides()
        {
            // Arrange
            var prefab = CreateInnerPrefab();
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, _root.transform);
            // Override a child, since the root Transform's position is only a default override.
            var child = instance.transform.GetChild(0);
            child.localPosition = new Vector3(1f, 2f, 3f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(child);

            // Act
            var response = _adapter.Duplicate(instance.GetInstanceID(), null);

            // Assert
            var clone = Find(response.instanceId);
            Assert.IsTrue(PrefabUtility.IsOutermostPrefabInstanceRoot(clone));
            Assert.AreSame(prefab, PrefabUtility.GetCorrespondingObjectFromSource(clone));
            Assert.AreEqual(new Vector3(1f, 2f, 3f), clone.transform.GetChild(0).localPosition);
            Assert.IsTrue(PrefabUtility.HasPrefabInstanceAnyOverrides(clone, false));
        }

        [Test]
        public void Duplicate_NestedPrefabInstance_KeepsLinkToNestedPrefab()
        {
            // Arrange
            CreateInnerPrefab();
            var outerTemp = new GameObject("Outer");
            var innerAsset = AssetDatabase.LoadAssetAtPath<GameObject>(InnerPrefabPath);
            PrefabUtility.InstantiatePrefab(innerAsset, outerTemp.transform);
            PrefabUtility.SaveAsPrefabAsset(outerTemp, OuterPrefabPath);
            Object.DestroyImmediate(outerTemp);

            var outerAsset = AssetDatabase.LoadAssetAtPath<GameObject>(OuterPrefabPath);
            var outerInstance = (GameObject)PrefabUtility.InstantiatePrefab(outerAsset, _root.transform);
            var nestedInstance = outerInstance.transform.GetChild(0).gameObject;

            // Act
            var response = _adapter.Duplicate(nestedInstance.GetInstanceID(), null);

            // Assert
            var clone = Find(response.instanceId);
            Assert.AreSame(outerInstance.transform, clone.transform.parent);
            Assert.IsTrue(PrefabUtility.IsAnyPrefabInstanceRoot(clone));
            Assert.AreEqual(InnerPrefabPath, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(clone));
        }
    }
}
