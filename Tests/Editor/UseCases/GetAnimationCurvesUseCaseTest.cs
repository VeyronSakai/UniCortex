using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetAnimationCurvesUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsGetCurves_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyAnimationClipOperations
            {
                GetCurvesResult = new GetAnimationCurvesResponse(60f, true, 1f, new List<AnimationCurveEntry>())
            };
            var useCase = new GetAnimationCurvesUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync("Assets/Animations/FadeIn.anim", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.GetCurvesCallCount);
            Assert.AreEqual("Assets/Animations/FadeIn.anim", ops.LastGetCurvesAssetPath);
            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.AreEqual(60f, result.frameRate);
            Assert.IsTrue(result.loop);
        }
    }
}
