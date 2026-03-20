using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class AddTimelineTrackUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsAddTrack_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new AddTimelineTrackUseCase(dispatcher, ops);

            useCase.ExecuteAsync(12345, "UnityEngine.Timeline.AnimationTrack", "MyTrack", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.AddTrackCallCount);
            Assert.AreEqual(12345, ops.LastAddTrackInstanceId);
            Assert.AreEqual("UnityEngine.Timeline.AnimationTrack", ops.LastAddTrackType);
            Assert.AreEqual("MyTrack", ops.LastAddTrackName);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
