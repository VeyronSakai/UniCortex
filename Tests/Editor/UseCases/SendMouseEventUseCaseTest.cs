using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.Domains.Models;
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
            var ops = new SpyInputOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, ops);

            useCase.ExecuteAsync(100f, 200f, MouseButton.Left, InputEventType.Press, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.SendMouseEventCallCount);
            Assert.AreEqual(100f, ops.LastMouseX);
            Assert.AreEqual(200f, ops.LastMouseY);
            Assert.AreEqual(MouseButton.Left, ops.LastMouseButton);
            Assert.AreEqual(InputEventType.Press, ops.LastMouseEventType);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_Click_DecomposesIntoPressAndRelease()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, ops);

            useCase.ExecuteAsync(150f, 250f, MouseButton.Left, InputEventType.Click, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(2, ops.SendMouseEventCallCount);
            Assert.AreEqual(2, dispatcher.CallCount);

            Assert.AreEqual(150f, ops.MouseEventHistory[0].X);
            Assert.AreEqual(250f, ops.MouseEventHistory[0].Y);
            Assert.AreEqual(MouseButton.Left, ops.MouseEventHistory[0].Button);
            Assert.AreEqual(InputEventType.Press, ops.MouseEventHistory[0].EventType);

            Assert.AreEqual(150f, ops.MouseEventHistory[1].X);
            Assert.AreEqual(250f, ops.MouseEventHistory[1].Y);
            Assert.AreEqual(MouseButton.Left, ops.MouseEventHistory[1].Button);
            Assert.AreEqual(InputEventType.Release, ops.MouseEventHistory[1].EventType);
        }
    }
}
