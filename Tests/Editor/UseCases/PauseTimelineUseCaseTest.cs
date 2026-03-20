using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class PauseTimelineUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsPauseTimeline_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new PauseTimelineUseCase(dispatcher, ops);

            useCase.ExecuteAsync(12345, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.PauseTimelineCallCount);
            Assert.AreEqual(12345, ops.LastPauseInstanceId);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
