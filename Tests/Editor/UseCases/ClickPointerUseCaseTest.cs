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
    internal sealed class ClickPointerUseCaseTest
    {
        private static ClickPointerUseCase CreateUseCase(FakePlayerLoopDispatcher playerLoopDispatcher,
            SpyInputOperations ops, SpyPointerTargetOperations pointerTargetOps = null)
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var resolver = new PointerPositionResolver(dispatcher,
                pointerTargetOps ?? new SpyPointerTargetOperations());
            return new ClickPointerUseCase(dispatcher, playerLoopDispatcher, resolver, ops);
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

        [Test]
        public void ExecuteAsync_PressesAndReleasesInNextFrame_WhenHoldDurationIsZero()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = CreateUseCase(playerLoopDispatcher, ops);
            PointerResponse response = null;

            // Act
            var frames = RecordFrames(playerLoopDispatcher, ops, () =>
                response = useCase.ExecuteAsync(new PointerPosition.Coordinates(150f, 250f), MouseButton.Right,
                    0f, CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            Assert.AreEqual(3, frames.Count);
            var press = frames[0].Single();
            var release = frames[1].Single();
            CollectionAssert.IsEmpty(frames[2]);
            Assert.AreEqual(MouseAction.Press, press.Action);
            Assert.AreEqual(MouseAction.Release, release.Action);
            foreach (var record in new[] { press, release })
            {
                Assert.AreEqual(150f, record.X);
                Assert.AreEqual(250f, record.Y);
                Assert.AreEqual(MouseButton.Right, record.Button);
            }

            Assert.IsTrue(response.success);
            Assert.AreEqual(150f, response.x);
            Assert.AreEqual(250f, response.y);
        }

        [Test]
        public void ExecuteAsync_KeepsPressed_ForHoldDuration()
        {
            // Arrange
            // Frames are 0.25 seconds apart.
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = CreateUseCase(playerLoopDispatcher, ops);

            // Act
            var frames = RecordFrames(playerLoopDispatcher, ops, () =>
                useCase.ExecuteAsync(new PointerPosition.Coordinates(0f, 0f), MouseButton.Left, 0.6f,
                    CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            // Pressed at 0s, released at 0.75s (the first frame at least 0.6s after the press).
            Assert.AreEqual(5, frames.Count);
            Assert.AreEqual(MouseAction.Press, frames[0].Single().Action);
            CollectionAssert.IsEmpty(frames[1]);
            CollectionAssert.IsEmpty(frames[2]);
            Assert.AreEqual(MouseAction.Release, frames[3].Single().Action);
            CollectionAssert.IsEmpty(frames[4]);
        }

        [Test]
        public void ExecuteAsync_ClicksAtTargetCenter()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations { TargetCenterToReturn = (40f, 60f) };
            var useCase = CreateUseCase(playerLoopDispatcher, ops, pointerTargetOps);

            // Act
            var response = useCase.ExecuteAsync(new PointerPosition.Target(12345), MouseButton.Left, 0f,
                CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(12345, pointerTargetOps.LastInstanceId);
            Assert.AreEqual(2, ops.MouseEventHistory.Count);
            Assert.AreEqual(40f, ops.MouseEventHistory[1].X);
            Assert.AreEqual(60f, ops.MouseEventHistory[1].Y);
            Assert.AreEqual(40f, response.x);
            Assert.AreEqual(60f, response.y);
        }

        [Test]
        public void ExecuteAsync_Throws_WhenHoldDurationIsNegative()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = CreateUseCase(playerLoopDispatcher, ops);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => useCase.ExecuteAsync(
                new PointerPosition.Coordinates(0f, 0f), MouseButton.Left, -0.1f, CancellationToken.None)
                .GetAwaiter().GetResult());
            StringAssert.StartsWith("holdDuration ", ex.Message);
            Assert.AreEqual(0, playerLoopDispatcher.RunEachFrameCallCount);
            CollectionAssert.IsEmpty(ops.MouseEventHistory);
        }
    }
}
