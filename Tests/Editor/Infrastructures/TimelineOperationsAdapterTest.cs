using System;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class TimelineOperationsAdapterTest
    {
        private const string TimelinePath = "Assets/TimelineOperationsAdapterTest.playable";
        private const string AnimationClipPath = "Assets/TimelineOperationsAdapterTest.anim";
        private const string AnimationTrackType = "UnityEngine.Timeline.AnimationTrack";
        private const string ControlTrackType = "UnityEngine.Timeline.ControlTrack";

        private TimelineOperationsAdapter _adapter;
        private GameObject _gameObject;
        private int _instanceId;

        [SetUp]
        public void SetUp()
        {
            _adapter = new TimelineOperationsAdapter();
            _adapter.CreateTimeline(TimelinePath);

            _gameObject = new GameObject("TimelineOperationsAdapterTest");
            var director = _gameObject.AddComponent<PlayableDirector>();
            director.playableAsset = AssetDatabase.LoadAssetAtPath<PlayableAsset>(TimelinePath);
            _instanceId = _gameObject.GetInstanceID();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_gameObject);
            AssetDatabase.DeleteAsset(TimelinePath);
            AssetDatabase.DeleteAsset(AnimationClipPath);
        }

        [Test]
        public void GetTracks_ReturnsTracksClipsAndBinding_WhenReadByInstanceId()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, AnimationTrackType, "Anim");
            _adapter.AddClip(_instanceId, 0, 1.0, 2.0, "Fade");
            _adapter.BindTrack(_instanceId, 0, _instanceId);

            // Act
            var result = _adapter.GetTracks(_instanceId, null);

            // Assert
            Assert.AreEqual(TimelinePath, result.assetPath);
            Assert.AreEqual(1, result.tracks.Count);
            var track = result.tracks[0];
            Assert.AreEqual(0, track.index);
            Assert.AreEqual("Anim", track.name);
            Assert.AreEqual(AnimationTrackType, track.type);
            Assert.AreEqual(_instanceId, track.bindingInstanceId);
            Assert.AreEqual("TimelineOperationsAdapterTest", track.bindingName);
            Assert.AreEqual(1, track.clips.Count);
            var clip = track.clips[0];
            Assert.AreEqual("Fade", clip.displayName);
            Assert.AreEqual("UnityEngine.Timeline.AnimationPlayableAsset", clip.assetType);
            Assert.AreEqual(1.0, clip.start, 1e-6);
            Assert.AreEqual(2.0, clip.duration, 1e-6);
            Assert.AreEqual("", clip.animationClipPath);
        }

        [Test]
        public void GetTracks_ReturnsTracksWithoutBinding_WhenReadByAssetPath()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, AnimationTrackType, "Anim");
            _adapter.BindTrack(_instanceId, 0, _instanceId);

            // Act
            var result = _adapter.GetTracks(0, TimelinePath);

            // Assert
            Assert.AreEqual(1, result.tracks.Count);
            Assert.AreEqual(0, result.tracks[0].bindingInstanceId);
        }

        [Test]
        public void GetTracks_Throws_WhenAssetPathNotFound()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _adapter.GetTracks(0, "Assets/NotFound.playable"));
        }

        [Test]
        public void GetTrackProperties_ReturnsTypeAndProperties()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, AnimationTrackType, "Anim");

            // Act
            var result = _adapter.GetTrackProperties(_instanceId, null, 0);

            // Assert
            Assert.AreEqual(AnimationTrackType, result.typeName);
            Assert.IsTrue(result.properties.Exists(p => p.path == "m_TrackOffset"));
            Assert.IsFalse(result.properties.Exists(p => p.path == "m_Script"));
        }

        [Test]
        public void GetTrackProperties_Throws_WhenTrackIndexOutOfRange()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _adapter.GetTrackProperties(_instanceId, null, 0));
        }

        [Test]
        public void GetClipProperties_ReturnsAssetTypeAndProperties()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, AnimationTrackType, "Anim");
            _adapter.AddClip(_instanceId, 0, 0, 2.0, "Fade");

            // Act
            var result = _adapter.GetClipProperties(_instanceId, null, 0, 0);

            // Assert
            Assert.AreEqual("UnityEngine.Timeline.AnimationPlayableAsset", result.typeName);
            Assert.IsTrue(result.properties.Exists(p => p.path == "m_Clip"));
        }

        [Test]
        public void GetClipProperties_LeavesExposedReferenceUnresolved_WhenReadByAssetPath()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, ControlTrackType, "Control");
            _adapter.AddClip(_instanceId, 0, 0, 2.0, "Control");
            _adapter.SetClipProperty(_instanceId, 0, 0, "sourceGameObject", _instanceId.ToString());

            // Act
            var result = _adapter.GetClipProperties(0, TimelinePath, 0, 0);

            // Assert
            var property = result.properties.Find(p => p.path == "sourceGameObject");
            Assert.AreEqual("null", property.value);
        }

        [Test]
        public void GetClipProperties_Throws_WhenClipIndexOutOfRange()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, AnimationTrackType, "Anim");

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _adapter.GetClipProperties(_instanceId, null, 0, 0));
        }

        [Test]
        public void ModifyClip_ChangesOnlyGivenValues()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, AnimationTrackType, "Anim");
            _adapter.AddClip(_instanceId, 0, 1.0, 2.0, "Fade");
            var request = new ModifyTimelineClipRequest
            {
                instanceId = _instanceId,
                trackIndex = 0,
                clipIndex = 0,
                start = 0.5,
                timeScale = 2.0,
                easeInDuration = 0.25,
                preExtrapolation = TimelineClipExtrapolationModes.None,
                postExtrapolation = TimelineClipExtrapolationModes.Loop
            };

            // Act
            _adapter.ModifyClip(request);

            // Assert
            var clip = _adapter.GetTracks(_instanceId, null).tracks[0].clips[0];
            Assert.AreEqual(0.5, clip.start, 1e-6);
            Assert.AreEqual(2.0, clip.duration, 1e-6);
            Assert.AreEqual(2.0, clip.timeScale, 1e-6);
            Assert.AreEqual(0.25, clip.easeInDuration, 1e-6);
            Assert.AreEqual(TimelineClipExtrapolationModes.None, clip.preExtrapolation);
            Assert.AreEqual(TimelineClipExtrapolationModes.Loop, clip.postExtrapolation);
            Assert.AreEqual("Fade", clip.displayName);
        }

        [Test]
        public void ModifyClip_Throws_WhenDurationIsNotPositive()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, AnimationTrackType, "Anim");
            _adapter.AddClip(_instanceId, 0, 0, 2.0, "Fade");
            var request = new ModifyTimelineClipRequest
                { instanceId = _instanceId, trackIndex = 0, clipIndex = 0, duration = 0 };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _adapter.ModifyClip(request));
        }

        [Test]
        public void ModifyClip_Throws_WhenExtrapolationIsInvalid()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, AnimationTrackType, "Anim");
            _adapter.AddClip(_instanceId, 0, 0, 2.0, "Fade");
            var request = new ModifyTimelineClipRequest
                { instanceId = _instanceId, trackIndex = 0, clipIndex = 0, postExtrapolation = "Forever" };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _adapter.ModifyClip(request));
        }

        [Test]
        public void ModifyClip_Throws_WhenClipDoesNotSupportExtrapolation()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, ControlTrackType, "Control");
            _adapter.AddClip(_instanceId, 0, 0, 2.0, "Control");
            var request = new ModifyTimelineClipRequest
            {
                instanceId = _instanceId, trackIndex = 0, clipIndex = 0,
                postExtrapolation = TimelineClipExtrapolationModes.Loop
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _adapter.ModifyClip(request));
        }

        [Test]
        public void ModifyClip_Throws_WhenClipIndexOutOfRange()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, AnimationTrackType, "Anim");
            var request = new ModifyTimelineClipRequest { instanceId = _instanceId, trackIndex = 0, clipIndex = 0 };

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _adapter.ModifyClip(request));
        }

        [Test]
        public void SetClipProperty_AssignsAnimationClip_ThroughClipProperty()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, AnimationTrackType, "Anim");
            _adapter.AddClip(_instanceId, 0, 0, 2.0, "Fade");
            AssetDatabase.CreateAsset(new AnimationClip(), AnimationClipPath);

            // Act
            _adapter.SetClipProperty(_instanceId, 0, 0, "m_Clip", AnimationClipPath);

            // Assert
            var clip = _adapter.GetTracks(_instanceId, null).tracks[0].clips[0];
            Assert.AreEqual(AnimationClipPath, clip.animationClipPath);
        }

        [Test]
        public void SetClipProperty_SetsValue()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, ControlTrackType, "Control");
            _adapter.AddClip(_instanceId, 0, 0, 2.0, "Control");

            // Act
            _adapter.SetClipProperty(_instanceId, 0, 0, "active", "false");

            // Assert
            var result = _adapter.GetClipProperties(_instanceId, null, 0, 0);
            var property = result.properties.Find(p => p.path == "active");
            Assert.AreEqual("false", property.value);
        }

        [Test]
        public void SetClipProperty_ResolvesExposedReferenceThroughDirector()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, ControlTrackType, "Control");
            _adapter.AddClip(_instanceId, 0, 0, 2.0, "Control");

            // Act
            _adapter.SetClipProperty(_instanceId, 0, 0, "sourceGameObject", _instanceId.ToString());

            // Assert
            var result = _adapter.GetClipProperties(_instanceId, null, 0, 0);
            var property = result.properties.Find(p => p.path == "sourceGameObject");
            Assert.AreEqual(_instanceId.ToString(), property.value);
        }

        [Test]
        public void SetClipProperty_Throws_WhenPropertyNotFound()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, ControlTrackType, "Control");
            _adapter.AddClip(_instanceId, 0, 0, 2.0, "Control");

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => _adapter.SetClipProperty(_instanceId, 0, 0, "notExisting", "1"));
        }

        [Test]
        public void SetTrackProperty_MutesTrack()
        {
            // Arrange
            _adapter.AddTrack(_instanceId, AnimationTrackType, "Anim");

            // Act
            _adapter.SetTrackProperty(_instanceId, 0, "m_Muted", "true");

            // Assert
            Assert.IsTrue(_adapter.GetTracks(_instanceId, null).tracks[0].muted);
        }

        [Test]
        public void SetTrackProperty_Throws_WhenTrackIndexOutOfRange()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _adapter.SetTrackProperty(_instanceId, 0, "m_Muted", "true"));
        }
    }
}
