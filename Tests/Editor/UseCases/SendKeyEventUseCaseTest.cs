using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SendKeyEventUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSendKeyEvent_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputSimulationOperations();
            var useCase = new SendKeyEventUseCase(dispatcher, ops);

            useCase.ExecuteAsync("space", "keyDown", CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.SendKeyEventCallCount);
            Assert.AreEqual("space", ops.LastKeyName);
            Assert.AreEqual("keyDown", ops.LastKeyEventType);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
