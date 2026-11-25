using System.Linq;
using NUnit.Framework;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class SerializedPropertyListerTest
    {
        private SerializeReferenceScriptableObject _so;

        [SetUp]
        public void SetUp()
        {
            _so = ScriptableObject.CreateInstance<SerializeReferenceScriptableObject>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_so);
        }

        [Test]
        public void List_SerializeReferenceCycle_ListsEachValueOnce()
        {
            // Arrange: head -> second -> head
            var head = new SerializeReferenceScriptableObject.Node { value = 1 };
            var second = new SerializeReferenceScriptableObject.Node { value = 2, next = head };
            head.next = second;
            _so.head = head;

            // Act
            var properties = SerializedPropertyLister.List(new SerializedObject(_so), null, null);

            // Assert
            var paths = properties.Select(p => p.path).ToList();
            CollectionAssert.Contains(paths, "head.value");
            CollectionAssert.Contains(paths, "head.next.value");
            // head.next.next is head again, so it is listed but not expanded.
            CollectionAssert.Contains(paths, "head.next.next");
            CollectionAssert.DoesNotContain(paths, "head.next.next.value");
        }

        [Test]
        public void List_SerializeReferenceToItself_ListsItOnce()
        {
            // Arrange
            var head = new SerializeReferenceScriptableObject.Node { value = 1 };
            head.next = head;
            _so.head = head;

            // Act
            var properties = SerializedPropertyLister.List(new SerializedObject(_so), null, null);

            // Assert
            var paths = properties.Select(p => p.path).ToList();
            CollectionAssert.Contains(paths, "head.next");
            CollectionAssert.DoesNotContain(paths, "head.next.value");
        }
    }
}
