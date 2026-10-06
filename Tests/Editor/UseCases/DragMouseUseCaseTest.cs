using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using MouseAction = UniCortex.Editor.Tests.TestDoubles.SpyInputOperations.MouseAction;
using MouseEventRecord = UniCortex.Editor.Tests.TestDoubles.SpyInputOperations.MouseEventRecord;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class DragMouseUseCaseTest
    {
        private static DragMouseUseCase CreateUseCase(FakePlayerLoopDispatcher playerLoopDispatcher,
            SpyInputOperations ops, SpyPointerTargetOperations pointerTargetOps = null)
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var resolver = new PointerPositionResolver(dispatcher,
                pointerTargetOps ?? new SpyPointerTargetOperations());
            return new DragMouseUseCase(dispatcher, playerLoopDispatcher, resolver, ops);
        }

        // Splits the sent mouse events by the frame they were sent in.
        private static List<List<MouseEventRecord>> RecordFrames(FakePlayerLoopDispatcher playerLoopDispatcher,
            SpyInputOperations ops, Action act)
        {
            var frameStarts = new List<int>();
            playerLoopDispatcher.OnFrame = _ => frameStarts.Add(ops.MouseEventHistory.Count);
            act();

            var frames = new List<List<MouseEventRecord>>();
            for (var i = 0; i < frameStarts.Count; i++)
            {
                var end = i + 1 < frameStarts.Count ? frameStarts[i + 1] : ops.MouseEventHistory.Count;
                frames.Add(ops.MouseEventHistory.GetRange(frameStarts[i], end - frameStarts[i]));
            }

            return frames;
        }

        private static void AssertEvent(MouseEventRecord record, MouseAction action, float x, float y)
        {
            Assert.AreEqual(action, record.Action);
            Assert.AreEqual(x, record.X, 0.0001f);
            Assert.AreEqual(y, record.Y, 0.0001f);
        }

        [Test]
        public void ExecuteAsync_PressesMovesOverDurationAndReleases()
        {
            // Arrange
            // Frames are 0.25 seconds apart.
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = CreateUseCase(playerLoopDispatcher, ops);
            DragMouseResponse response = null;

            // Act
            var frames = RecordFrames(playerLoopDispatcher, ops, () =>
                response = useCase.ExecuteAsync(new PointerPosition.Coordinates(0f, 0f),
                    new PointerPosition.Coordinates(100f, 50f), MouseButton.Left, 1f,
                    CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            Assert.AreEqual(1, playerLoopDispatcher.RunEachFrameCallCount);
            Assert.AreEqual(7, frames.Count);
            AssertEvent(frames[0].Single(), MouseAction.Press, 0f, 0f);
            AssertEvent(frames[1].Single(), MouseAction.Move, 25f, 12.5f);
            AssertEvent(frames[2].Single(), MouseAction.Move, 50f, 25f);
            AssertEvent(frames[3].Single(), MouseAction.Move, 75f, 37.5f);
            AssertEvent(frames[4].Single(), MouseAction.Move, 100f, 50f);
            AssertEvent(frames[5].Single(), MouseAction.Release, 100f, 50f);
            CollectionAssert.IsEmpty(frames[6]);
            Assert.AreEqual(MouseButton.Left, frames[0].Single().Button);
            Assert.AreEqual(MouseButton.Left, frames[5].Single().Button);
            Assert.IsTrue(response.success);
            Assert.AreEqual(0f, response.fromX);
            Assert.AreEqual(0f, response.fromY);
            Assert.AreEqual(100f, response.toX);
            Assert.AreEqual(50f, response.toY);
        }

        [Test]
        public void ExecuteAsync_MovesToEndInOneFrame_WhenDurationIsZero()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = CreateUseCase(playerLoopDispatcher, ops);

            // Act
            var frames = RecordFrames(playerLoopDispatcher, ops, () =>
                useCase.ExecuteAsync(new PointerPosition.Coordinates(0f, 0f),
                    new PointerPosition.Coordinates(100f, 0f), MouseButton.Left, 0f,
                    CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            Assert.AreEqual(4, frames.Count);
            AssertEvent(frames[0].Single(), MouseAction.Press, 0f, 0f);
            AssertEvent(frames[1].Single(), MouseAction.Move, 100f, 0f);
            AssertEvent(frames[2].Single(), MouseAction.Release, 100f, 0f);
            CollectionAssert.IsEmpty(frames[3]);
        }

        [Test]
        public void ExecuteAsync_WithTargets_DragsBetweenTargetCenters()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations();
            pointerTargetOps.TargetCentersToReturn[111] = (10f, 20f);
            pointerTargetOps.TargetCentersToReturn[222] = (110f, 220f);
            var useCase = CreateUseCase(playerLoopDispatcher, ops, pointerTargetOps);

            // Act
            var response = useCase.ExecuteAsync(new PointerPosition.Target(111), new PointerPosition.Target(222),
                MouseButton.Left, 0.5f, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(2, pointerTargetOps.GetTargetCenterCallCount);
            Assert.AreEqual(4, ops.MouseEventHistory.Count);
            AssertEvent(ops.MouseEventHistory[0], MouseAction.Press, 10f, 20f);
            AssertEvent(ops.MouseEventHistory[1], MouseAction.Move, 60f, 120f);
            AssertEvent(ops.MouseEventHistory[2], MouseAction.Move, 110f, 220f);
            AssertEvent(ops.MouseEventHistory[3], MouseAction.Release, 110f, 220f);
            Assert.AreEqual(10f, response.fromX);
            Assert.AreEqual(20f, response.fromY);
            Assert.AreEqual(110f, response.toX);
            Assert.AreEqual(220f, response.toY);
        }

        [Test]
        public void ExecuteAsync_Throws_WhenDurationIsNegative()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = CreateUseCase(playerLoopDispatcher, ops);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => useCase.ExecuteAsync(
                new PointerPosition.Coordinates(0f, 0f), new PointerPosition.Coordinates(1f, 1f),
                MouseButton.Left, -0.1f, CancellationToken.None).GetAwaiter().GetResult());
            StringAssert.StartsWith("duration ", ex.Message);
            Assert.AreEqual(0, playerLoopDispatcher.RunEachFrameCallCount);
            CollectionAssert.IsEmpty(ops.MouseEventHistory);
        }
    }
}
