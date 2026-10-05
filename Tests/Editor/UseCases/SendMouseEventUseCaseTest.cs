using System;
using System.Collections.Generic;
using System.Linq;
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
            var useCase = new SendMouseEventUseCase(dispatcher, new FakePlayerLoopDispatcher(), ops,
                new SpyPointerTargetOperations());

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
        }

        [Test]
        public void ExecuteAsync_Click_DecomposesIntoPressAndRelease()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, new FakePlayerLoopDispatcher(), ops,
                new SpyPointerTargetOperations());

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
            var pointerTargetOps = new SpyPointerTargetOperations { TargetCenterToReturn = (320f, 180f) };
            var useCase = new SendMouseEventUseCase(dispatcher, new FakePlayerLoopDispatcher(), ops,
                pointerTargetOps);

            // Act
            var response = useCase.ExecuteAsync(12345, MouseButton.Left, InputEventType.Press,
                CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, pointerTargetOps.GetTargetCenterCallCount);
            Assert.AreEqual(12345, pointerTargetOps.LastInstanceId);
            Assert.AreEqual(1, ops.SendMouseEventCallCount);
            Assert.AreEqual(320f, ops.LastMouseX);
            Assert.AreEqual(180f, ops.LastMouseY);
            Assert.AreEqual(InputEventType.Press, ops.LastMouseEventType);
            Assert.AreEqual(2, dispatcher.CallCount);
            Assert.AreEqual(320f, response.x);
            Assert.AreEqual(180f, response.y);
        }

        [Test]
        public void ExecuteAsync_WithTarget_Click_SendsPressAndReleaseToTargetCenter()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations { TargetCenterToReturn = (40f, 60f) };
            var useCase = new SendMouseEventUseCase(dispatcher, new FakePlayerLoopDispatcher(), ops,
                pointerTargetOps);

            // Act
            useCase.ExecuteAsync(12345, MouseButton.Left, InputEventType.Click, CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(2, ops.SendMouseEventCallCount);
            Assert.AreEqual(40f, ops.MouseEventHistory[0].X);
            Assert.AreEqual(60f, ops.MouseEventHistory[0].Y);
            Assert.AreEqual(InputEventType.Press, ops.MouseEventHistory[0].EventType);
            Assert.AreEqual(40f, ops.MouseEventHistory[1].X);
            Assert.AreEqual(60f, ops.MouseEventHistory[1].Y);
            Assert.AreEqual(InputEventType.Release, ops.MouseEventHistory[1].EventType);
        }

        // Splits the sent mouse events by the frame they were sent in.
        // frameStarts holds the number of events sent before each frame.
        private static List<List<SpyInputOperations.MouseEventRecord>> SplitByFrame(
            List<SpyInputOperations.MouseEventRecord> history, List<int> frameStarts)
        {
            var frames = new List<List<SpyInputOperations.MouseEventRecord>>();
            for (var i = 0; i < frameStarts.Count; i++)
            {
                var end = i + 1 < frameStarts.Count ? frameStarts[i + 1] : history.Count;
                frames.Add(history.GetRange(frameStarts[i], end - frameStarts[i]));
            }

            return frames;
        }

        private static void AssertEvent(SpyInputOperations.MouseEventRecord record, float x, float y,
            string eventType)
        {
            Assert.AreEqual(x, record.X, 0.0001f);
            Assert.AreEqual(y, record.Y, 0.0001f);
            Assert.AreEqual(eventType, record.EventType);
        }

        [Test]
        public void DragAsync_PressesMovesOncePerFrameAndReleases()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, playerLoopDispatcher, ops,
                new SpyPointerTargetOperations());
            var frameStarts = new List<int>();
            playerLoopDispatcher.OnFrame = _ => frameStarts.Add(ops.MouseEventHistory.Count);

            // Act
            var response = useCase.DragAsync(MousePosition.At(0f, 0f), MousePosition.At(100f, 50f),
                MouseButton.Left, 4, 0, CancellationToken.None).GetAwaiter().GetResult();
            var frames = SplitByFrame(ops.MouseEventHistory, frameStarts);

            // Assert
            Assert.AreEqual(1, playerLoopDispatcher.RunEachFrameCallCount);
            Assert.AreEqual(7, frames.Count);
            AssertEvent(frames[0].Single(), 0f, 0f, InputEventType.Press);
            AssertEvent(frames[1].Single(), 25f, 12.5f, InputEventType.Move);
            AssertEvent(frames[2].Single(), 50f, 25f, InputEventType.Move);
            AssertEvent(frames[3].Single(), 75f, 37.5f, InputEventType.Move);
            AssertEvent(frames[4].Single(), 100f, 50f, InputEventType.Move);
            AssertEvent(frames[5].Single(), 100f, 50f, InputEventType.Release);
            CollectionAssert.IsEmpty(frames[6]);
            Assert.IsTrue(ops.MouseEventHistory.TrueForAll(e => e.Button == MouseButton.Left));
            Assert.IsTrue(response.success);
            Assert.AreEqual(0f, response.x);
            Assert.AreEqual(0f, response.y);
            Assert.AreEqual(100f, response.toX);
            Assert.AreEqual(50f, response.toY);
        }

        [Test]
        public void DragAsync_KeepsPressed_ForHoldFrames_BeforeMoving()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, playerLoopDispatcher, ops,
                new SpyPointerTargetOperations());
            var frameStarts = new List<int>();
            playerLoopDispatcher.OnFrame = _ => frameStarts.Add(ops.MouseEventHistory.Count);

            // Act
            useCase.DragAsync(MousePosition.At(10f, 20f), MousePosition.At(30f, 40f),
                MouseButton.Right, 1, 3, CancellationToken.None).GetAwaiter().GetResult();
            var frames = SplitByFrame(ops.MouseEventHistory, frameStarts);

            // Assert
            Assert.AreEqual(7, frames.Count);
            AssertEvent(frames[0].Single(), 10f, 20f, InputEventType.Press);
            CollectionAssert.IsEmpty(frames[1]);
            CollectionAssert.IsEmpty(frames[2]);
            CollectionAssert.IsEmpty(frames[3]);
            AssertEvent(frames[4].Single(), 30f, 40f, InputEventType.Move);
            AssertEvent(frames[5].Single(), 30f, 40f, InputEventType.Release);
            CollectionAssert.IsEmpty(frames[6]);
            Assert.IsTrue(ops.MouseEventHistory.TrueForAll(e => e.Button == MouseButton.Right));
        }

        [Test]
        public void DragAsync_WithTargets_DragsBetweenTargetCenters()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations();
            pointerTargetOps.TargetCentersToReturn[111] = (10f, 20f);
            pointerTargetOps.TargetCentersToReturn[222] = (110f, 220f);
            var useCase = new SendMouseEventUseCase(dispatcher, playerLoopDispatcher, ops, pointerTargetOps);

            // Act
            var response = useCase.DragAsync(MousePosition.CenterOf(111), MousePosition.CenterOf(222),
                MouseButton.Left, 2, 0, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(2, pointerTargetOps.GetTargetCenterCallCount);
            Assert.AreEqual(4, ops.SendMouseEventCallCount);
            AssertEvent(ops.MouseEventHistory[0], 10f, 20f, InputEventType.Press);
            AssertEvent(ops.MouseEventHistory[1], 60f, 120f, InputEventType.Move);
            AssertEvent(ops.MouseEventHistory[2], 110f, 220f, InputEventType.Move);
            AssertEvent(ops.MouseEventHistory[3], 110f, 220f, InputEventType.Release);
            Assert.AreEqual(10f, response.x);
            Assert.AreEqual(20f, response.y);
            Assert.AreEqual(110f, response.toX);
            Assert.AreEqual(220f, response.toY);
        }

        [TestCase(0, 0, "frames")]
        [TestCase(1, -1, "holdFrames")]
        public void DragAsync_Throws_WhenFrameCountIsInvalid(int frames, int holdFrames, string parameterName)
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, playerLoopDispatcher, ops,
                new SpyPointerTargetOperations());

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => useCase.DragAsync(MousePosition.At(0f, 0f),
                MousePosition.At(1f, 1f), MouseButton.Left, frames, holdFrames, CancellationToken.None)
                .GetAwaiter().GetResult());
            StringAssert.StartsWith(parameterName + " ", ex.Message);
            Assert.AreEqual(0, playerLoopDispatcher.RunEachFrameCallCount);
            Assert.AreEqual(0, ops.SendMouseEventCallCount);
        }
    }
}
