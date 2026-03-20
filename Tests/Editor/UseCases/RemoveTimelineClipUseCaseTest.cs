using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class RemoveTimelineClipUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsRemoveClip_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new RemoveTimelineClipUseCase(dispatcher, ops);

            useCase.ExecuteAsync(12345, 1, 2, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.RemoveClipCallCount);
            Assert.AreEqual(12345, ops.LastRemoveClipInstanceId);
            Assert.AreEqual(1, ops.LastRemoveClipTrackIndex);
            Assert.AreEqual(2, ops.LastRemoveClipIndex);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
