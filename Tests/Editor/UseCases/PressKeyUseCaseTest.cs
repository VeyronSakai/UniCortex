using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using KeyAction = UniCortex.Editor.Tests.TestDoubles.SpyInputOperations.KeyAction;
using KeyEventRecord = UniCortex.Editor.Tests.TestDoubles.SpyInputOperations.KeyEventRecord;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class PressKeyUseCaseTest
    {
        private static PressKeyUseCase CreateUseCase(FakePlayerLoopDispatcher playerLoopDispatcher,
            SpyInputOperations ops)
        {
            return new PressKeyUseCase(
                new PlayerLoopRunner(new FakeMainThreadDispatcher(), playerLoopDispatcher), ops,
                playerLoopDispatcher.Time);
        }

        // Splits the sent key events by the frame they were sent in.
        private static List<List<KeyEventRecord>> RecordFrames(FakePlayerLoopDispatcher playerLoopDispatcher,
            SpyInputOperations ops, Action act)
        {
            var frameStarts = new List<int>();
            playerLoopDispatcher.OnFrame = _ => frameStarts.Add(ops.KeyEventHistory.Count);
            act();

            var frames = new List<List<KeyEventRecord>>();
            for (var i = 0; i < frameStarts.Count; i++)
            {
                var end = i + 1 < frameStarts.Count ? frameStarts[i + 1] : ops.KeyEventHistory.Count;
                frames.Add(ops.KeyEventHistory.GetRange(frameStarts[i], end - frameStarts[i]));
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
            var keys = new[] { KeyName.Space };

            // Act
            var frames = RecordFrames(playerLoopDispatcher, ops, () =>
                useCase.ExecuteAsync(keys, 0f, CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            Assert.AreEqual(3, frames.Count);
            var press = frames[0].Single();
            var release = frames[1].Single();
            CollectionAssert.IsEmpty(frames[2]);
            Assert.AreEqual(KeyAction.Press, press.Action);
            Assert.AreEqual(KeyAction.Release, release.Action);
            CollectionAssert.AreEqual(keys, press.Keys);
            CollectionAssert.AreEqual(keys, release.Keys);
        }

        [Test]
        public void ExecuteAsync_PressesAndReleasesAllKeysTogether()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = CreateUseCase(playerLoopDispatcher, ops);
            var keys = new[] { KeyName.LeftCtrl, KeyName.S };

            // Act
            useCase.ExecuteAsync(keys, 0f, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(2, ops.KeyEventHistory.Count);
            CollectionAssert.AreEqual(keys, ops.KeyEventHistory[0].Keys);
            CollectionAssert.AreEqual(keys, ops.KeyEventHistory[1].Keys);
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
                useCase.ExecuteAsync(new[] { KeyName.W }, 0.6f, CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            // Pressed at 0s, released at 0.75s (the first frame at least 0.6s after the press).
            Assert.AreEqual(5, frames.Count);
            Assert.AreEqual(KeyAction.Press, frames[0].Single().Action);
            CollectionAssert.IsEmpty(frames[1]);
            CollectionAssert.IsEmpty(frames[2]);
            Assert.AreEqual(KeyAction.Release, frames[3].Single().Action);
            CollectionAssert.IsEmpty(frames[4]);
        }

        [Test]
        public void ExecuteAsync_Throws_WhenKeysIsEmpty()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = CreateUseCase(playerLoopDispatcher, ops);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => useCase.ExecuteAsync(
                Array.Empty<string>(), 0f, CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(PressKeyUseCase.KeysRequiredMessage, ex.Message);
            Assert.AreEqual(0, playerLoopDispatcher.FrameCount);
            CollectionAssert.IsEmpty(ops.KeyEventHistory);
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
                new[] { KeyName.A }, -0.1f, CancellationToken.None).GetAwaiter().GetResult());
            StringAssert.StartsWith("holdDuration ", ex.Message);
            Assert.AreEqual(0, playerLoopDispatcher.FrameCount);
            CollectionAssert.IsEmpty(ops.KeyEventHistory);
        }

        [Test]
        public void ExecuteAsync_BlocksPhysicalKeyboard_UntilReleaseIsProcessed()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = CreateUseCase(playerLoopDispatcher, ops);
            var blockedInLastFrame = false;

            // Act
            playerLoopDispatcher.OnFrame = _ => blockedInLastFrame = ops.PhysicalKeyboardBlockCount > 0;
            useCase.ExecuteAsync(new[] { KeyName.A }, 0f, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(2, ops.KeyEventHistory.Count);
            Assert.IsTrue(ops.KeyEventHistory.All(record => record.PhysicalKeyboardBlocked));
            Assert.IsTrue(blockedInLastFrame);
            Assert.AreEqual(0, ops.PhysicalKeyboardBlockCount);
        }

        [Test]
        public void ExecuteAsync_UnblocksPhysicalKeyboard_WhenReleaseFails()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations { ExceptionOnRelease = new InvalidOperationException("failed") };
            var useCase = CreateUseCase(playerLoopDispatcher, ops);

            // Act
            Assert.Throws<InvalidOperationException>(() => useCase.ExecuteAsync(
                new[] { KeyName.A }, 0f, CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            Assert.AreEqual(0, ops.PhysicalKeyboardBlockCount);
        }
    }
}
