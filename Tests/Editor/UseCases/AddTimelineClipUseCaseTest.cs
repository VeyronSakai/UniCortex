using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class AddTimelineClipUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsAddClip_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new AddTimelineClipUseCase(dispatcher, ops);

            useCase.ExecuteAsync(12345, 0, 1.0, 3.0, "MyClip", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.AddClipCallCount);
            Assert.AreEqual(12345, ops.LastAddClipInstanceId);
            Assert.AreEqual(0, ops.LastAddClipTrackIndex);
            Assert.AreEqual(1.0, ops.LastAddClipStart, 0.001);
            Assert.AreEqual(3.0, ops.LastAddClipDuration, 0.001);
            Assert.AreEqual("MyClip", ops.LastAddClipName);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
