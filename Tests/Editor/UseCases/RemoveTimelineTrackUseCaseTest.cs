using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class RemoveTimelineTrackUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsRemoveTrack_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new RemoveTimelineTrackUseCase(dispatcher, ops);

            useCase.ExecuteAsync(12345, 2, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.RemoveTrackCallCount);
            Assert.AreEqual(12345, ops.LastRemoveTrackInstanceId);
            Assert.AreEqual(2, ops.LastRemoveTrackIndex);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
