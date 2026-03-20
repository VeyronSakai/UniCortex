using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetTimelineTimeUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSetTimelineTime_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new SetTimelineTimeUseCase(dispatcher, ops);

            useCase.ExecuteAsync(12345, 5.5, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.SetTimelineTimeCallCount);
            Assert.AreEqual(12345, ops.LastSetTimeInstanceId);
            Assert.AreEqual(5.5, ops.LastSetTimeValue, 0.001);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
