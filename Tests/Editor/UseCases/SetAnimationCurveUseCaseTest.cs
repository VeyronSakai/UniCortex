using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetAnimationCurveUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSetCurve_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyAnimationClipOperations();
            var useCase = new SetAnimationCurveUseCase(dispatcher, ops);
            var keys = new List<AnimationCurveKeyInput>
            {
                new AnimationCurveKeyInput { time = 0f, value = 0f },
                new AnimationCurveKeyInput { time = 1f, value = 1f, tangentMode = "Linear" }
            };

            useCase.ExecuteAsync("Assets/Animations/FadeIn.anim", "Root/Child", "UnityEngine.UI.Image",
                "UnityEngine.UI", "m_Color.a", keys, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.SetCurveCallCount);
            Assert.AreEqual("Assets/Animations/FadeIn.anim", ops.LastSetCurveAssetPath);
            Assert.AreEqual("Root/Child", ops.LastSetCurveAnimatorRelativePath);
            Assert.AreEqual("UnityEngine.UI.Image", ops.LastSetCurveComponentType);
            Assert.AreEqual("UnityEngine.UI", ops.LastSetCurveAssemblyName);
            Assert.AreEqual("m_Color.a", ops.LastSetCurvePropertyName);
            Assert.AreSame(keys, ops.LastSetCurveKeys);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
