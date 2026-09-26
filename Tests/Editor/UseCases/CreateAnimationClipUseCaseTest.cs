using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class CreateAnimationClipUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsCreate_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyAnimationClipOperations();
            var useCase = new CreateAnimationClipUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync("Assets/Animations/FadeIn.anim", true, 30f, CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.CreateCallCount);
            Assert.AreEqual("Assets/Animations/FadeIn.anim", ops.LastCreateAssetPath);
            Assert.IsTrue(ops.LastCreateLoop);
            Assert.AreEqual(30f, ops.LastCreateFrameRate);
            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.IsTrue(result.success);
            Assert.AreEqual("Assets/Animations/FadeIn.anim", result.assetPath);
        }
    }
}
