using System;
using System.Linq;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class AudioMixerOperationsAdapterTest
    {
        private const string TestFolder = "Assets/AudioMixerOperationsAdapterTest";
        private const string TestAssetPath = TestFolder + "/Test.mixer";

        private AudioMixerOperationsAdapter _adapter;
        private GameObject _gameObject;
        private int _undoGroup;

        [SetUp]
        public void SetUp()
        {
            // Start a new Undo group so TearDown can revert the scene changes the adapter records.
            Undo.IncrementCurrentGroup();
            _undoGroup = Undo.GetCurrentGroup();
            _adapter = new AudioMixerOperationsAdapter();
        }

        [TearDown]
        public void TearDown()
        {
            Undo.RevertAllDownToGroup(_undoGroup);
            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
            }

            AssetDatabase.DeleteAsset(TestFolder);
        }

        [Test]
        public void Create_CreatesMixerWithMasterGroupAndSnapshot()
        {
            // Act
            var response = _adapter.Create(TestAssetPath);

            // Assert
            Assert.IsTrue(response.success);
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<AudioMixer>(TestAssetPath));
            var mixer = _adapter.Get(TestAssetPath);
            Assert.AreEqual(new[] { "Master" }, mixer.groups.Select(g => g.path).ToArray());
            Assert.AreEqual(1, mixer.snapshots.Count);
            Assert.IsTrue(mixer.snapshots[0].isStartSnapshot);
            Assert.IsEmpty(mixer.exposedParameters);
        }

        [Test]
        public void Create_Throws_WhenExtensionIsNotMixer()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _adapter.Create(TestFolder + "/Test.asset"));
        }

        [Test]
        public void Create_Throws_WhenAssetAlreadyExists()
        {
            // Arrange
            _adapter.Create(TestAssetPath);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _adapter.Create(TestAssetPath));
        }

        [Test]
        public void AddGroup_AddsGroupsUnderMasterAndNestedParent()
        {
            // Arrange
            _adapter.Create(TestAssetPath);

            // Act
            var music = _adapter.AddGroup(TestAssetPath, "Music", null);
            var bgm = _adapter.AddGroup(TestAssetPath, "BGM", "Music");

            // Assert
            Assert.AreEqual("Master/Music", music.path);
            Assert.AreEqual("Master/Music/BGM", bgm.path);
            var groups = _adapter.Get(TestAssetPath).groups;
            Assert.AreEqual(new[] { "Master", "Master/Music", "Master/Music/BGM" },
                groups.Select(g => g.path).ToArray());
            var runtimeMixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(TestAssetPath);
            Assert.AreEqual(1, runtimeMixer.FindMatchingGroups("Master/Music/BGM").Length);
        }

        [Test]
        public void AddGroup_Throws_WhenSiblingWithSameNameExists()
        {
            // Arrange
            _adapter.Create(TestAssetPath);
            _adapter.AddGroup(TestAssetPath, "Music", "Master");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _adapter.AddGroup(TestAssetPath, "Music", "Master"));
        }

        [Test]
        public void AddGroup_Throws_WhenParentNotFound()
        {
            // Arrange
            _adapter.Create(TestAssetPath);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _adapter.AddGroup(TestAssetPath, "Music", "Missing"));
            StringAssert.Contains("Master", ex.Message);
        }

        [Test]
        public void ExposeParameter_ExposesVolumeUnderGivenName()
        {
            // Arrange
            _adapter.Create(TestAssetPath);
            _adapter.AddGroup(TestAssetPath, "Music", null);

            // Act
            _adapter.ExposeParameter(TestAssetPath, "Music", "volume", "MusicVolume");

            // Assert
            var parameter = _adapter.Get(TestAssetPath).exposedParameters.Single();
            Assert.AreEqual("MusicVolume", parameter.name);
            Assert.AreEqual("Master/Music", parameter.groupPath);
            Assert.AreEqual(AudioMixerParameters.Volume, parameter.parameter);

            var runtimeMixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(TestAssetPath);
            Assert.IsTrue(runtimeMixer.GetFloat("MusicVolume", out _));
        }

        [Test]
        public void ExposeParameter_ExposesPitch()
        {
            // Arrange
            _adapter.Create(TestAssetPath);

            // Act
            _adapter.ExposeParameter(TestAssetPath, "Master", "Pitch", "MasterPitch");

            // Assert
            var parameter = _adapter.Get(TestAssetPath).exposedParameters.Single();
            Assert.AreEqual("MasterPitch", parameter.name);
            Assert.AreEqual(AudioMixerParameters.Pitch, parameter.parameter);
        }

        [Test]
        public void ExposeParameter_RenamesAlreadyExposedParameter()
        {
            // Arrange
            _adapter.Create(TestAssetPath);
            _adapter.ExposeParameter(TestAssetPath, "Master", "volume", "OldName");

            // Act
            _adapter.ExposeParameter(TestAssetPath, "Master", "volume", "NewName");

            // Assert
            var parameter = _adapter.Get(TestAssetPath).exposedParameters.Single();
            Assert.AreEqual("NewName", parameter.name);
        }

        [Test]
        public void ExposeParameter_Throws_WhenNameIsUsedByAnotherParameter()
        {
            // Arrange
            _adapter.Create(TestAssetPath);
            _adapter.ExposeParameter(TestAssetPath, "Master", "volume", "Shared");

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _adapter.ExposeParameter(TestAssetPath, "Master", "pitch", "Shared"));
        }

        [Test]
        public void ExposeParameter_Throws_WhenParameterIsUnsupported()
        {
            // Arrange
            _adapter.Create(TestAssetPath);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _adapter.ExposeParameter(TestAssetPath, "Master", "send", "Send"));
        }

        [Test]
        public void SetAudioSourceOutput_RoutesAudioSourceToGroup()
        {
            // Arrange
            _adapter.Create(TestAssetPath);
            _adapter.AddGroup(TestAssetPath, "Effect", null);
            _gameObject = new GameObject("AudioMixerOperationsAdapterTest");
            var source = _gameObject.AddComponent<AudioSource>();

            // Act
            _adapter.SetAudioSourceOutput(_gameObject.GetInstanceID(), 0, TestAssetPath, "Master/Effect");

            // Assert
            Assert.IsNotNull(source.outputAudioMixerGroup);
            Assert.AreEqual("Effect", source.outputAudioMixerGroup.name);
            Assert.AreEqual(AssetDatabase.LoadAssetAtPath<AudioMixer>(TestAssetPath),
                source.outputAudioMixerGroup.audioMixer);
        }

        [Test]
        public void SetAudioSourceOutput_Throws_WhenGameObjectHasNoAudioSource()
        {
            // Arrange
            _adapter.Create(TestAssetPath);
            _gameObject = new GameObject("AudioMixerOperationsAdapterTest");

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _adapter.SetAudioSourceOutput(_gameObject.GetInstanceID(), 0, TestAssetPath, "Master"));
        }

        [Test]
        public void Get_Throws_WhenMixerNotFound()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _adapter.Get(TestFolder + "/Missing.mixer"));
        }
    }
}
