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
            var response = _adapter.Create(TestAssetPath, true, 30f);

            Assert.IsTrue(response.success);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(TestAssetPath);
            Assert.IsNotNull(clip);
            Assert.AreEqual(30f, clip.frameRate);
            Assert.IsTrue(AnimationUtility.GetAnimationClipSettings(clip).loopTime);
        }

        [Test]
        public void Create_UsesDefaultFrameRate_WhenZero()
        {
            _adapter.Create(TestAssetPath, false, 0f);

            var result = _adapter.GetCurves(TestAssetPath);
            Assert.AreEqual(60f, result.frameRate);
            Assert.IsFalse(result.loop);
        }

        [Test]
        public void Create_Throws_WhenExtensionIsNotAnim()
        {
            Assert.Throws<ArgumentException>(() =>
                _adapter.Create("Assets/AnimationClipOperationsAdapterTest.asset", false, 60f));
        }

        [Test]
        public void SetCurve_ThenGetCurves_ReturnsKeys()
        {
            _adapter.Create(TestAssetPath, false, 60f);

            _adapter.SetCurve(TestAssetPath, "Root/Child", TransformType, CoreModule, "m_LocalScale.x",
                new List<AnimationCurveKeyInput>
                {
                    new AnimationCurveKeyInput { time = 0f, value = 0f },
                    new AnimationCurveKeyInput { time = 0.5f, value = 1f }
                });

            var result = _adapter.GetCurves(TestAssetPath);

            Assert.AreEqual(1, result.curves.Count);
            var curve = result.curves[0];
            Assert.AreEqual("Root/Child", curve.path);
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
            _adapter.Create(TestAssetPath, false, 60f);
            _adapter.SetCurve(TestAssetPath, "", TransformType, CoreModule, "m_LocalPosition.y",
                new List<AnimationCurveKeyInput>
                {
                    new AnimationCurveKeyInput { time = 0f, value = 0f },
                    new AnimationCurveKeyInput { time = 1f, value = 1f },
                    new AnimationCurveKeyInput { time = 2f, value = 0f }
                });

            _adapter.SetCurve(TestAssetPath, "", TransformType, CoreModule, "m_LocalPosition.y",
                new List<AnimationCurveKeyInput> { new AnimationCurveKeyInput { time = 0f, value = 5f } });

            var result = _adapter.GetCurves(TestAssetPath);
            Assert.AreEqual(1, result.curves.Count);
            Assert.AreEqual(1, result.curves[0].keys.Count);
            Assert.AreEqual(5f, result.curves[0].keys[0].value);
        }

        [Test]
        public void SetCurve_Throws_WhenTypeNotFound()
        {
            _adapter.Create(TestAssetPath, false, 60f);

            Assert.Throws<ArgumentException>(() => _adapter.SetCurve(TestAssetPath, "", "UnityEngine.Transform",
                "Assembly-CSharp", "m_LocalScale.x",
                new List<AnimationCurveKeyInput> { new AnimationCurveKeyInput { time = 0f, value = 1f } }));
        }

        [Test]
        public void SetCurve_Throws_WhenClipNotFound()
        {
            Assert.Throws<ArgumentException>(() => _adapter.SetCurve("Assets/NotExisting.anim", "",
                TransformType, CoreModule, "m_LocalScale.x",
                new List<AnimationCurveKeyInput> { new AnimationCurveKeyInput { time = 0f, value = 1f } }));
        }

        [Test]
        public void RemoveCurve_RemovesCurve()
        {
            _adapter.Create(TestAssetPath, false, 60f);
            _adapter.SetCurve(TestAssetPath, "", TransformType, CoreModule, "m_LocalScale.x",
                new List<AnimationCurveKeyInput> { new AnimationCurveKeyInput { time = 0f, value = 1f } });
            _adapter.SetCurve(TestAssetPath, "", TransformType, CoreModule, "m_LocalScale.y",
                new List<AnimationCurveKeyInput> { new AnimationCurveKeyInput { time = 0f, value = 1f } });

            _adapter.RemoveCurve(TestAssetPath, "", TransformType, CoreModule, "m_LocalScale.x");

            var result = _adapter.GetCurves(TestAssetPath);
            Assert.AreEqual(1, result.curves.Count);
            Assert.AreEqual("m_LocalScale.y", result.curves[0].propertyName);
        }

        [Test]
        public void RemoveCurve_Throws_WhenCurveNotFound()
        {
            _adapter.Create(TestAssetPath, false, 60f);

            Assert.Throws<ArgumentException>(() =>
                _adapter.RemoveCurve(TestAssetPath, "", TransformType, CoreModule, "m_LocalScale.x"));
        }

        [Test]
        public void GetCurves_ConstantKeys_HaveInfiniteTangentsWrittenAsBareTokens()
        {
            _adapter.Create(TestAssetPath, false, 60f);
            _adapter.SetCurve(TestAssetPath, "", "UnityEngine.GameObject", CoreModule, "m_IsActive",
                new List<AnimationCurveKeyInput>
                {
                    new AnimationCurveKeyInput { time = 0f, value = 1f, tangentMode = "Constant" },
                    new AnimationCurveKeyInput { time = 0.5f, value = 0f, tangentMode = "Constant" }
                });

            var key = _adapter.GetCurves(TestAssetPath).curves[0].keys[0];

            Assert.IsTrue(float.IsPositiveInfinity(key.outTangent));
            // The Core relies on JsonUtility writing infinity as a bare token.
            StringAssert.Contains("\"outTangent\":Infinity", JsonUtility.ToJson(key));
        }

        [Test]
        public void JsonUtility_ReadsBareInfinityTokens()
        {
            // The Core sends infinite tangents as bare tokens; JsonUtility must read them back.
            var key = JsonUtility.FromJson<AnimationCurveKeyInput>(
                "{\"time\":0,\"value\":1,\"inTangent\":-Infinity,\"outTangent\":Infinity}");

            Assert.IsTrue(float.IsNegativeInfinity(key.inTangent));
            Assert.IsTrue(float.IsPositiveInfinity(key.outTangent));
        }

        [Test]
        public void BuildCurve_InfiniteTangent_HoldsValueUntilNextKey()
        {
            var curve = AnimationClipOperationsAdapter.BuildCurve(new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 1f, outTangent = float.PositiveInfinity },
                new AnimationCurveKeyInput { time = 1f, value = 0f, inTangent = float.PositiveInfinity }
            });

            Assert.AreEqual(1f, curve.Evaluate(0.5f));
        }

        [Test]
        public void BuildCurve_SortsKeysByTime()
        {
            var curve = AnimationClipOperationsAdapter.BuildCurve(new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 1f, value = 1f },
                new AnimationCurveKeyInput { time = 0f, value = 0f }
            });

            Assert.AreEqual(0f, curve[0].time);
            Assert.AreEqual(1f, curve[1].time);
        }

        [Test]
        public void BuildCurve_Linear_ComputesSlopeAndSetsMode()
        {
            var curve = AnimationClipOperationsAdapter.BuildCurve(new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 0f, tangentMode = "Linear" },
                new AnimationCurveKeyInput { time = 2f, value = 1f, tangentMode = "linear" }
            });

            Assert.AreEqual(0.5f, curve[0].outTangent, 1e-4f);
            Assert.AreEqual(0.5f, curve[1].inTangent, 1e-4f);
            Assert.AreEqual(AnimationUtility.TangentMode.Linear, AnimationUtility.GetKeyRightTangentMode(curve, 0));
            Assert.AreEqual(AnimationUtility.TangentMode.Linear, AnimationUtility.GetKeyLeftTangentMode(curve, 1));
        }

        [Test]
        public void BuildCurve_Constant_SetsConstantMode()
        {
            var curve = AnimationClipOperationsAdapter.BuildCurve(new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 0f, tangentMode = "Constant" },
                new AnimationCurveKeyInput { time = 1f, value = 1f, tangentMode = "Constant" }
            });

            Assert.AreEqual(0f, curve.Evaluate(0.5f));
            Assert.AreEqual(AnimationUtility.TangentMode.Constant, AnimationUtility.GetKeyRightTangentMode(curve, 0));
        }

        [Test]
        public void BuildCurve_Free_KeepsGivenTangents()
        {
            var curve = AnimationClipOperationsAdapter.BuildCurve(new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 0f, inTangent = 0f, outTangent = 3f },
                new AnimationCurveKeyInput { time = 1f, value = 1f, inTangent = -2f, outTangent = 0f }
            });

            Assert.AreEqual(3f, curve[0].outTangent);
            Assert.AreEqual(-2f, curve[1].inTangent);
            Assert.AreEqual(AnimationUtility.TangentMode.Free, AnimationUtility.GetKeyRightTangentMode(curve, 0));
        }

        [Test]
        public void BuildCurve_DefaultsToFreeWithFlatTangents_WhenOmitted()
        {
            var curve = AnimationClipOperationsAdapter.BuildCurve(new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 0f },
                new AnimationCurveKeyInput { time = 1f, value = 1f }
            });

            Assert.AreEqual(0f, curve[0].outTangent);
            Assert.AreEqual(0f, curve[1].inTangent);
            Assert.AreEqual(AnimationUtility.TangentMode.Free, AnimationUtility.GetKeyRightTangentMode(curve, 0));
        }

        [Test]
        public void BuildCurve_Throws_WhenTangentModeUnsupported()
        {
            Assert.Throws<ArgumentException>(() => AnimationClipOperationsAdapter.BuildCurve(
                new List<AnimationCurveKeyInput>
                {
                    new AnimationCurveKeyInput { time = 0f, value = 0f, tangentMode = "Bouncy" }
                }));
        }

        [Test]
        public void BuildCurve_Throws_WhenDuplicateTimes()
        {
            Assert.Throws<ArgumentException>(() => AnimationClipOperationsAdapter.BuildCurve(
                new List<AnimationCurveKeyInput>
                {
                    new AnimationCurveKeyInput { time = 0.5f, value = 0f },
                    new AnimationCurveKeyInput { time = 0.5f, value = 1f }
                }));
        }

        [Test]
        public void BuildCurve_Throws_WhenKeysEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                AnimationClipOperationsAdapter.BuildCurve(new List<AnimationCurveKeyInput>()));
        }
    }
}
