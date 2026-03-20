using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetTimelineBindingUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSetBinding_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyTimelineOperations();
            var useCase = new SetTimelineBindingUseCase(dispatcher, ops);

            useCase.ExecuteAsync(12345, 1, 67890, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.SetBindingCallCount);
            Assert.AreEqual(12345, ops.LastSetBindingInstanceId);
            Assert.AreEqual(1, ops.LastSetBindingTrackIndex);
            Assert.AreEqual(67890, ops.LastSetBindingTargetInstanceId);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
