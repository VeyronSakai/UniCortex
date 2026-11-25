using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class ComponentOperationsAdapterGetPropertiesTest
    {
        private const string DirectorType = "UnityEngine.Playables.PlayableDirector";
        private const string DirectorAssembly = "UnityEngine.DirectorModule";
        private const string JointType = "UnityEngine.FixedJoint";
        private const string JointAssembly = "UnityEngine.PhysicsModule";
        private const string BindingValuePath = "m_SceneBindings.Array.data[0].value";
        private const string PrefabPath = "Assets/ComponentOperationsAdapterGetPropertiesTest.prefab";
        private const string AdditiveScenePath = "Assets/ComponentOperationsAdapterGetPropertiesTest.unity";

        private ComponentOperationsAdapter _adapter;
        private GameObject _root;
        private PlayableDirector _director;
        private GameObject _target;

        [SetUp]
        public void SetUp()
        {
            _adapter = new ComponentOperationsAdapter();
            _root = new GameObject("GetPropertiesTest");
            _director = CreateChild("Director").AddComponent<PlayableDirector>();
            _target = CreateChild("Target");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            AssetDatabase.DeleteAsset(PrefabPath);
        }

        private GameObject CreateChild(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root.transform, false);
            return go;
        }

        private List<SerializedPropertyEntry> GetDirectorProperties(string propertyPath = null, int? depth = null)
        {
            return _adapter.GetProperties(_director.gameObject.GetInstanceID(), DirectorType, DirectorAssembly, 0,
                propertyPath, depth).properties;
        }

        [Test]
        public void GetProperties_ListsArrayElements_WithPathsAndReference()
        {
            // Arrange
            _director.SetGenericBinding(CreateChild("Key"), _target);

            // Act
            var properties = GetDirectorProperties();

            // Assert
            var size = properties.Single(p => p.path == "m_SceneBindings.Array.size");
            Assert.AreEqual("1", size.value);
            var binding = properties.Single(p => p.path == BindingValuePath);
            Assert.AreEqual("ObjectReference", binding.type);
            Assert.AreEqual(_target.GetInstanceID().ToString(), binding.value);
            Assert.AreEqual("GetPropertiesTest/Target (UnityEngine.GameObject)", binding.reference);
        }

        [Test]
        public void GetProperties_WithDepthZero_ListsOnlyTopLevelProperties()
        {
            // Arrange
            _director.SetGenericBinding(CreateChild("Key"), _target);

            // Act
            var properties = GetDirectorProperties(depth: 0);

            // Assert
            CollectionAssert.Contains(properties.Select(p => p.path).ToList(), "m_SceneBindings");
            Assert.IsTrue(properties.All(p => !p.path.Contains(".")));
        }

        [Test]
        public void GetProperties_WithPropertyPath_ListsOnlyThatSubtree()
        {
            // Arrange
            _director.SetGenericBinding(CreateChild("Key"), _target);

            // Act
            var properties = GetDirectorProperties("m_SceneBindings");

            // Assert
            Assert.AreEqual("m_SceneBindings", properties[0].path);
            Assert.IsTrue(properties.All(p => p.path.StartsWith("m_SceneBindings", StringComparison.Ordinal)));
            CollectionAssert.Contains(properties.Select(p => p.path).ToList(), BindingValuePath);
        }

        [Test]
        public void GetProperties_WithPropertyPathAndDepth_StopsAtThatDepth()
        {
            // Arrange
            _director.SetGenericBinding(CreateChild("Key"), _target);

            // Act
            var properties = GetDirectorProperties("m_SceneBindings", 1);

            // Assert
            var paths = properties.Select(p => p.path).ToList();
            CollectionAssert.Contains(paths, "m_SceneBindings.Array.data[0]");
            CollectionAssert.DoesNotContain(paths, BindingValuePath);
        }

        [Test]
        public void GetProperties_WithPropertyPathOfLeaf_ReturnsOnlyThatProperty()
        {
            // Arrange
            _director.SetGenericBinding(CreateChild("Key"), _target);

            // Act
            var properties = GetDirectorProperties(BindingValuePath);

            // Assert
            Assert.AreEqual(1, properties.Count);
            Assert.AreEqual(BindingValuePath, properties[0].path);
        }

        [Test]
        public void GetProperties_WithUnknownPropertyPath_Throws()
        {
            // Act
            var ex = Assert.Throws<ArgumentException>(() => GetDirectorProperties("m_NoSuchProperty"));

            // Assert
            StringAssert.Contains("Property 'm_NoSuchProperty' not found", ex.Message);
        }

        [Test]
        public void GetProperties_ExpandsNestedValues_ButNotTheCharactersOfStrings()
        {
            // Arrange: ParticleSystem has nested modules that hold strings.
            var particles = CreateChild("Particles").AddComponent<ParticleSystem>();
            var instanceId = particles.gameObject.GetInstanceID();
            const string type = "UnityEngine.ParticleSystem";
            const string assembly = "UnityEngine.ParticleSystemModule";

            // Act
            var all = _adapter.GetProperties(instanceId, type, assembly, 0, null, null).properties;
            var topLevel = _adapter.GetProperties(instanceId, type, assembly, 0, null, 0).properties;

            // Assert
            Assert.Greater(all.Count, topLevel.Count);
            var strings = all.Where(p => p.type == "String").Select(p => p.path + ".").ToList();
            Assert.IsNotEmpty(strings);
            Assert.IsFalse(all.Any(p => strings.Any(s => p.path.StartsWith(s, StringComparison.Ordinal))));
        }

        [Test]
        public void GetProperties_ReferenceToDestroyedObject_IsMissing()
        {
            // Arrange
            var joint = CreateChild("Joint").AddComponent<FixedJoint>();
            var body = new GameObject("Body").AddComponent<Rigidbody>();
            joint.connectedBody = body;
            Object.DestroyImmediate(body.gameObject);

            // Act
            var properties = _adapter.GetProperties(joint.gameObject.GetInstanceID(), JointType, JointAssembly, 0,
                "m_ConnectedBody", null).properties;

            // Assert
            Assert.AreEqual("null", properties[0].value);
            Assert.AreEqual("Missing", properties[0].reference);
        }

        [Test]
        public void GetProperties_ReferencesIntoAnAsset_NameTheAssetAndTheObjectInIt()
        {
            // Arrange
            var temp = new GameObject("PrefabRoot");
            new GameObject("Body", typeof(Rigidbody)).transform.SetParent(temp.transform, false);
            var prefab = PrefabUtility.SaveAsPrefabAsset(temp, PrefabPath);
            Object.DestroyImmediate(temp);
            var joint = CreateChild("Joint").AddComponent<FixedJoint>();
            joint.connectedBody = prefab.transform.GetChild(0).GetComponent<Rigidbody>();
            _director.SetGenericBinding(CreateChild("Key"), prefab);

            // Act
            var body = _adapter.GetProperties(joint.gameObject.GetInstanceID(), JointType, JointAssembly, 0,
                "m_ConnectedBody", null).properties[0];
            var binding = GetDirectorProperties(BindingValuePath)[0];

            // Assert
            Assert.AreEqual(PrefabPath, body.value);
            var prefabName = prefab.name;
            Assert.AreEqual($"{PrefabPath}: {prefabName}/Body (UnityEngine.Rigidbody)", body.reference);
            Assert.AreEqual($"{PrefabPath} (UnityEngine.GameObject)", binding.reference);
        }

        [Test]
        public void GetProperties_ReferenceToObjectInAnotherScene_NamesThatScene()
        {
            // Arrange
            var additiveScene = AdditiveTestScene.Open(AdditiveScenePath);
            try
            {
                var other = new GameObject("InOtherScene");
                SceneManager.MoveGameObjectToScene(other, additiveScene);
                _director.SetGenericBinding(CreateChild("Key"), other);

                // Act
                var binding = GetDirectorProperties(BindingValuePath)[0];

                // Assert
                Assert.AreEqual($"{AdditiveScenePath}: InOtherScene (UnityEngine.GameObject)", binding.reference);
            }
            finally
            {
                AdditiveTestScene.Close(additiveScene, AdditiveScenePath);
            }
        }

        [Test]
        public void GetProperties_PropertiesThatAreNotReferences_HaveNoReference()
        {
            // Act
            var properties = GetDirectorProperties(depth: 0);

            // Assert
            Assert.IsNull(properties.Single(p => p.path == "m_InitialTime").reference);
            Assert.IsNull(properties.Single(p => p.path == "m_PlayableAsset").reference);
        }
    }
}
