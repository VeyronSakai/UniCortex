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
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, ops, new SpyPointerTargetOperations());

            // Act
            var response = useCase.ExecuteAsync(100f, 200f, MouseButton.Left, InputEventType.Press,
                CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, ops.SendMouseEventCallCount);
            Assert.AreEqual(100f, ops.LastMouseX);
            Assert.AreEqual(200f, ops.LastMouseY);
            Assert.AreEqual(MouseButton.Left, ops.LastMouseButton);
            Assert.AreEqual(InputEventType.Press, ops.LastMouseEventType);
            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.IsTrue(response.success);
            Assert.AreEqual(100f, response.x);
            Assert.AreEqual(200f, response.y);
            Assert.IsFalse(response.targetBlocked);
        }

        [Test]
        public void ExecuteAsync_Click_DecomposesIntoPressAndRelease()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, ops, new SpyPointerTargetOperations());

            // Act
            useCase.ExecuteAsync(150f, 250f, MouseButton.Left, InputEventType.Click, CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
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

        [Test]
        public void ExecuteAsync_WithTarget_SendsEventToTargetCenter()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations
            {
                PointerTargetToReturn = SpyPointerTargetOperations.CreateTarget(320f, 180f, false, "")
            };
            var useCase = new SendMouseEventUseCase(dispatcher, ops, pointerTargetOps);

            // Act
            var response = useCase.ExecuteAsync(0, "Canvas/Button", MouseButton.Left, InputEventType.Press,
                CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, pointerTargetOps.GetPointerTargetCallCount);
            Assert.AreEqual(0, pointerTargetOps.LastInstanceId);
            Assert.AreEqual("Canvas/Button", pointerTargetOps.LastPath);
            Assert.AreEqual(1, ops.SendMouseEventCallCount);
            Assert.AreEqual(320f, ops.LastMouseX);
            Assert.AreEqual(180f, ops.LastMouseY);
            Assert.AreEqual(InputEventType.Press, ops.LastMouseEventType);
            Assert.AreEqual(2, dispatcher.CallCount);
            Assert.AreEqual(320f, response.x);
            Assert.AreEqual(180f, response.y);
            Assert.IsFalse(response.targetBlocked);
        }

        [Test]
        public void ExecuteAsync_WithTarget_Click_SendsPressAndReleaseToTargetCenter()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations
            {
                PointerTargetToReturn = SpyPointerTargetOperations.CreateTarget(40f, 60f, false, "")
            };
            var useCase = new SendMouseEventUseCase(dispatcher, ops, pointerTargetOps);

            // Act
            useCase.ExecuteAsync(12345, null, MouseButton.Left, InputEventType.Click, CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(12345, pointerTargetOps.LastInstanceId);
            Assert.AreEqual(2, ops.SendMouseEventCallCount);
            Assert.AreEqual(40f, ops.MouseEventHistory[0].X);
            Assert.AreEqual(60f, ops.MouseEventHistory[0].Y);
            Assert.AreEqual(InputEventType.Press, ops.MouseEventHistory[0].EventType);
            Assert.AreEqual(40f, ops.MouseEventHistory[1].X);
            Assert.AreEqual(60f, ops.MouseEventHistory[1].Y);
            Assert.AreEqual(InputEventType.Release, ops.MouseEventHistory[1].EventType);
        }

        [Test]
        public void ExecuteAsync_WithBlockedTarget_ReturnsBlockedBy()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations
            {
                PointerTargetToReturn = SpyPointerTargetOperations.CreateTarget(10f, 20f, true, "Canvas/Modal")
            };
            var useCase = new SendMouseEventUseCase(dispatcher, ops, pointerTargetOps);

            // Act
            var response = useCase.ExecuteAsync(100, null, MouseButton.Left, InputEventType.Click,
                CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.IsTrue(response.targetBlocked);
            Assert.AreEqual("Canvas/Modal", response.blockedBy);
            Assert.AreEqual(2, ops.SendMouseEventCallCount);
        }
    }
}
