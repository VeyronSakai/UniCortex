using System;
using System.Collections.Generic;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class AnimationClipOperationsAdapterTest
    {
        private const string TestAssetPath = "Assets/AnimationClipOperationsAdapterTest.anim";
        private const string TransformType = "UnityEngine.Transform";
        private const string CoreModule = "UnityEngine.CoreModule";

        private AnimationClipOperationsAdapter _adapter;

        [SetUp]
        public void SetUp()
        {
            _adapter = new AnimationClipOperationsAdapter();
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestAssetPath);
        }

        [Test]
        public void Create_CreatesClipWithLoopAndFrameRate()
        {
            // Act
            var response = _adapter.Create(TestAssetPath, true, 30f);

            // Assert
            Assert.IsTrue(response.success);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(TestAssetPath);
            Assert.IsNotNull(clip);
            Assert.AreEqual(30f, clip.frameRate);
            Assert.IsTrue(AnimationUtility.GetAnimationClipSettings(clip).loopTime);
        }

        [Test]
        public void Create_UsesDefaultFrameRate_WhenZero()
        {
            // Act
            _adapter.Create(TestAssetPath, false, 0f);

            // Assert
            var result = _adapter.GetCurves(TestAssetPath);
            Assert.AreEqual(60f, result.frameRate);
            Assert.IsFalse(result.loop);
        }

        [Test]
        public void Create_Throws_WhenExtensionIsNotAnim()
        {
            // Arrange
            const string assetPath = "Assets/AnimationClipOperationsAdapterTest.asset";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _adapter.Create(assetPath, false, 60f));
        }

        [Test]
        public void SetCurve_ThenGetCurves_ReturnsKeys()
        {
            // Arrange
            _adapter.Create(TestAssetPath, false, 60f);
            var keys = new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 0f },
                new AnimationCurveKeyInput { time = 0.5f, value = 1f }
            };

            // Act
            _adapter.SetCurve(TestAssetPath, "Root/Child", TransformType, CoreModule, "m_LocalScale.x", keys);
            var result = _adapter.GetCurves(TestAssetPath);

            // Assert
            Assert.AreEqual(1, result.curves.Count);
            var curve = result.curves[0];
            Assert.AreEqual("Root/Child", curve.animatorRelativePath);
            Assert.AreEqual(TransformType, curve.componentType);
            Assert.AreEqual(CoreModule, curve.assemblyName);
            Assert.AreEqual("m_LocalScale.x", curve.propertyName);
            Assert.AreEqual(2, curve.keys.Count);
            Assert.AreEqual(0.5f, curve.keys[1].time);
            Assert.AreEqual(1f, curve.keys[1].value);
            Assert.AreEqual(0.5f, result.length, 1e-4f);
        }

        [Test]
        public void SetCurve_ReplacesExistingKeys()
        {
            // Arrange
            _adapter.Create(TestAssetPath, false, 60f);
            _adapter.SetCurve(TestAssetPath, "", TransformType, CoreModule, "m_LocalPosition.y",
                new List<AnimationCurveKeyInput>
                {
                    new AnimationCurveKeyInput { time = 0f, value = 0f },
                    new AnimationCurveKeyInput { time = 1f, value = 1f },
                    new AnimationCurveKeyInput { time = 2f, value = 0f }
                });

            // Act
            _adapter.SetCurve(TestAssetPath, "", TransformType, CoreModule, "m_LocalPosition.y",
                new List<AnimationCurveKeyInput> { new AnimationCurveKeyInput { time = 0f, value = 5f } });

            // Assert
            var result = _adapter.GetCurves(TestAssetPath);
            Assert.AreEqual(1, result.curves.Count);
            Assert.AreEqual(1, result.curves[0].keys.Count);
            Assert.AreEqual(5f, result.curves[0].keys[0].value);
        }

        [Test]
        public void SetCurve_Throws_WhenTypeNotFound()
        {
            // Arrange
            _adapter.Create(TestAssetPath, false, 60f);
            var keys = new List<AnimationCurveKeyInput> { new AnimationCurveKeyInput { time = 0f, value = 1f } };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _adapter.SetCurve(TestAssetPath, "", TransformType,
                "Assembly-CSharp", "m_LocalScale.x", keys));
        }

        [Test]
        public void SetCurve_Throws_WhenClipNotFound()
        {
            // Arrange
            var keys = new List<AnimationCurveKeyInput> { new AnimationCurveKeyInput { time = 0f, value = 1f } };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _adapter.SetCurve("Assets/NotExisting.anim", "",
                TransformType, CoreModule, "m_LocalScale.x", keys));
        }

        [Test]
        public void RemoveCurve_RemovesCurve()
        {
            // Arrange
            _adapter.Create(TestAssetPath, false, 60f);
            _adapter.SetCurve(TestAssetPath, "", TransformType, CoreModule, "m_LocalScale.x",
                new List<AnimationCurveKeyInput> { new AnimationCurveKeyInput { time = 0f, value = 1f } });
            _adapter.SetCurve(TestAssetPath, "", TransformType, CoreModule, "m_LocalScale.y",
                new List<AnimationCurveKeyInput> { new AnimationCurveKeyInput { time = 0f, value = 1f } });

            // Act
            _adapter.RemoveCurve(TestAssetPath, "", TransformType, CoreModule, "m_LocalScale.x");

            // Assert
            var result = _adapter.GetCurves(TestAssetPath);
            Assert.AreEqual(1, result.curves.Count);
            Assert.AreEqual("m_LocalScale.y", result.curves[0].propertyName);
        }

        [Test]
        public void RemoveCurve_Throws_WhenCurveNotFound()
        {
            // Arrange
            _adapter.Create(TestAssetPath, false, 60f);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _adapter.RemoveCurve(TestAssetPath, "", TransformType, CoreModule, "m_LocalScale.x"));
        }

        [Test]
        public void GetCurves_ReportsInfiniteTangentsAsMaxValue()
        {
            // Arrange
            _adapter.Create(TestAssetPath, false, 60f);
            _adapter.SetCurve(TestAssetPath, "", "UnityEngine.GameObject", CoreModule, "m_IsActive",
                new List<AnimationCurveKeyInput>
                {
                    new AnimationCurveKeyInput { time = 0f, value = 1f, tangentMode = "Constant" },
                    new AnimationCurveKeyInput { time = 0.5f, value = 0f, tangentMode = "Constant" }
                });

            // Act
            var key = _adapter.GetCurves(TestAssetPath).curves[0].keys[0];

            // Assert
            Assert.AreEqual(float.MaxValue, key.outTangent);
            Assert.AreEqual("Constant", key.rightTangentMode);
            // JsonUtility would otherwise emit a bare "Infinity" token, which is not valid JSON.
            StringAssert.DoesNotContain("Infinity", JsonUtility.ToJson(key));
        }

        [Test]
        public void BuildCurve_MaxValueTangent_BecomesInfiniteAndHoldsValue()
        {
            // Arrange
            var keys = new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 1f, outTangent = float.MaxValue },
                new AnimationCurveKeyInput { time = 1f, value = 0f, inTangent = float.MaxValue }
            };

            // Act
            var curve = AnimationClipOperationsAdapter.BuildCurve(keys);

            // Assert
            Assert.IsTrue(float.IsPositiveInfinity(curve[0].outTangent));
            Assert.IsTrue(float.IsPositiveInfinity(curve[1].inTangent));
            Assert.AreEqual(1f, curve.Evaluate(0.5f));
        }

        [Test]
        public void ToSerializableTangent_ConvertsInfinityToMaxValue()
        {
            // Act
            var positive = AnimationClipOperationsAdapter.ToSerializableTangent(float.PositiveInfinity);
            var negative = AnimationClipOperationsAdapter.ToSerializableTangent(float.NegativeInfinity);
            var finite = AnimationClipOperationsAdapter.ToSerializableTangent(2.5f);

            // Assert
            Assert.AreEqual(float.MaxValue, positive);
            Assert.AreEqual(float.MinValue, negative);
            Assert.AreEqual(2.5f, finite);
        }

        [Test]
        public void FromSerializableTangent_ConvertsMaxValueToInfinity()
        {
            // Act
            var positive = AnimationClipOperationsAdapter.FromSerializableTangent(float.MaxValue);
            var negative = AnimationClipOperationsAdapter.FromSerializableTangent(float.MinValue);
            var finite = AnimationClipOperationsAdapter.FromSerializableTangent(-2.5f);

            // Assert
            Assert.IsTrue(float.IsPositiveInfinity(positive));
            Assert.IsTrue(float.IsNegativeInfinity(negative));
            Assert.AreEqual(-2.5f, finite);
        }

        [Test]
        public void BuildCurve_InfiniteTangent_HoldsValueUntilNextKey()
        {
            // Arrange
            var keys = new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 1f, outTangent = float.PositiveInfinity },
                new AnimationCurveKeyInput { time = 1f, value = 0f, inTangent = float.PositiveInfinity }
            };

            // Act
            var curve = AnimationClipOperationsAdapter.BuildCurve(keys);

            // Assert
            Assert.AreEqual(1f, curve.Evaluate(0.5f));
        }

        [Test]
        public void BuildCurve_SortsKeysByTime()
        {
            // Arrange
            var keys = new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 1f, value = 1f },
                new AnimationCurveKeyInput { time = 0f, value = 0f }
            };

            // Act
            var curve = AnimationClipOperationsAdapter.BuildCurve(keys);

            // Assert
            Assert.AreEqual(0f, curve[0].time);
            Assert.AreEqual(1f, curve[1].time);
        }

        [Test]
        public void BuildCurve_Linear_ComputesSlopeAndSetsMode()
        {
            // Arrange
            var keys = new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 0f, tangentMode = "Linear" },
                new AnimationCurveKeyInput { time = 2f, value = 1f, tangentMode = "linear" }
            };

            // Act
            var curve = AnimationClipOperationsAdapter.BuildCurve(keys);

            // Assert
            Assert.AreEqual(0.5f, curve[0].outTangent, 1e-4f);
            Assert.AreEqual(0.5f, curve[1].inTangent, 1e-4f);
            Assert.AreEqual(AnimationUtility.TangentMode.Linear, AnimationUtility.GetKeyRightTangentMode(curve, 0));
            Assert.AreEqual(AnimationUtility.TangentMode.Linear, AnimationUtility.GetKeyLeftTangentMode(curve, 1));
        }

        [Test]
        public void BuildCurve_Constant_SetsConstantMode()
        {
            // Arrange
            var keys = new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 0f, tangentMode = "Constant" },
                new AnimationCurveKeyInput { time = 1f, value = 1f, tangentMode = "Constant" }
            };

            // Act
            var curve = AnimationClipOperationsAdapter.BuildCurve(keys);

            // Assert
            Assert.AreEqual(0f, curve.Evaluate(0.5f));
            Assert.AreEqual(AnimationUtility.TangentMode.Constant, AnimationUtility.GetKeyRightTangentMode(curve, 0));
        }

        [Test]
        public void BuildCurve_Free_KeepsGivenTangents()
        {
            // Arrange
            var keys = new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 0f, inTangent = 0f, outTangent = 3f },
                new AnimationCurveKeyInput { time = 1f, value = 1f, inTangent = -2f, outTangent = 0f }
            };

            // Act
            var curve = AnimationClipOperationsAdapter.BuildCurve(keys);

            // Assert
            Assert.AreEqual(3f, curve[0].outTangent);
            Assert.AreEqual(-2f, curve[1].inTangent);
            Assert.AreEqual(AnimationUtility.TangentMode.Free, AnimationUtility.GetKeyRightTangentMode(curve, 0));
        }

        [Test]
        public void BuildCurve_DefaultsToFreeWithFlatTangents_WhenOmitted()
        {
            // Arrange
            var keys = new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 0f },
                new AnimationCurveKeyInput { time = 1f, value = 1f }
            };

            // Act
            var curve = AnimationClipOperationsAdapter.BuildCurve(keys);

            // Assert
            Assert.AreEqual(0f, curve[0].outTangent);
            Assert.AreEqual(0f, curve[1].inTangent);
            Assert.AreEqual(AnimationUtility.TangentMode.Free, AnimationUtility.GetKeyRightTangentMode(curve, 0));
        }

        [Test]
        public void BuildCurve_Throws_WhenTangentModeUnsupported()
        {
            // Arrange
            var keys = new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 0f, tangentMode = "Bouncy" }
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => AnimationClipOperationsAdapter.BuildCurve(keys));
        }

        [Test]
        public void BuildCurve_Throws_WhenDuplicateTimes()
        {
            // Arrange
            var keys = new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0.5f, value = 0f },
                new AnimationCurveKeyInput { time = 0.5f, value = 1f }
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => AnimationClipOperationsAdapter.BuildCurve(keys));
        }

        [Test]
        public void BuildCurve_Throws_WhenKeysEmpty()
        {
            // Arrange
            var keys = new List<AnimationCurveKeyInput>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => AnimationClipOperationsAdapter.BuildCurve(keys));
        }
    }
}
