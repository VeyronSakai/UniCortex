using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetTimelineInfoUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsGetTimelineInfo_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new GetTimelineInfoUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync(12345, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.GetTimelineInfoCallCount);
            Assert.AreEqual(12345, ops.LastGetInfoInstanceId);
            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.IsNotNull(result);
            Assert.AreEqual("TestTimeline", result.timelineAssetName);
        }
    }
}
