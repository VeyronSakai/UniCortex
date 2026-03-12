using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SendMouseEventUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSendMouseEvent_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputSimulationOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, ops);

            useCase.ExecuteAsync(100f, 200f, 0, "mouseDown", CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.SendMouseEventCallCount);
            Assert.AreEqual(100f, ops.LastMouseX);
            Assert.AreEqual(200f, ops.LastMouseY);
            Assert.AreEqual(0, ops.LastMouseButton);
            Assert.AreEqual("mouseDown", ops.LastMouseEventType);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
