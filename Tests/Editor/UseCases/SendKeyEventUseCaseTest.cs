using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.Domains.Models;
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

            useCase.ExecuteAsync(KeyName.Space, InputEventType.Press, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.SendKeyEventCallCount);
            Assert.AreEqual(KeyName.Space, ops.LastKey);
            Assert.AreEqual(InputEventType.Press, ops.LastKeyEventType);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
