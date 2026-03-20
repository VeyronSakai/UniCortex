using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class BindTimelineTrackUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsBindTrack_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new BindTimelineTrackUseCase(dispatcher, ops);

            useCase.ExecuteAsync(12345, 1, 67890, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.BindTrackCallCount);
            Assert.AreEqual(12345, ops.LastBindTrackInstanceId);
            Assert.AreEqual(1, ops.LastBindTrackTrackIndex);
            Assert.AreEqual(67890, ops.LastBindTrackTargetInstanceId);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
