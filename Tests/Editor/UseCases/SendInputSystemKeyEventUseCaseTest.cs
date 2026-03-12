using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SendInputSystemKeyEventUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSendKeyEvent_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputSystemSimulationOperations();
            var useCase = new SendInputSystemKeyEventUseCase(dispatcher, ops);

            useCase.ExecuteAsync("Space", "press", CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.SendKeyEventCallCount);
            Assert.AreEqual("Space", ops.LastKey);
            Assert.AreEqual("press", ops.LastKeyEventType);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
