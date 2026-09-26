using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class RemoveAnimationCurveUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsRemoveCurve_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyAnimationClipOperations();
            var useCase = new RemoveAnimationCurveUseCase(dispatcher, ops);

            useCase.ExecuteAsync("Assets/Animations/FadeIn.anim", "Root", "UnityEngine.Transform",
                "UnityEngine.CoreModule", "m_LocalScale.x", CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.RemoveCurveCallCount);
            Assert.AreEqual("Assets/Animations/FadeIn.anim", ops.LastRemoveCurveAssetPath);
            Assert.AreEqual("Root", ops.LastRemoveCurveAnimatorRelativePath);
            Assert.AreEqual("UnityEngine.Transform", ops.LastRemoveCurveComponentType);
            Assert.AreEqual("UnityEngine.CoreModule", ops.LastRemoveCurveAssemblyName);
            Assert.AreEqual("m_LocalScale.x", ops.LastRemoveCurvePropertyName);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
