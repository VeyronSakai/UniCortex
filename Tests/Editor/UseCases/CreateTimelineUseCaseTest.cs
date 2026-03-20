using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class CreateTimelineUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsCreateTimeline_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new CreateTimelineUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync("Assets/Test.playable", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.CreateTimelineCallCount);
            Assert.AreEqual("Assets/Test.playable", ops.LastCreateAssetPath);
            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.success);
            Assert.AreEqual("Assets/Test.playable", result.assetPath);
        }
    }
}
