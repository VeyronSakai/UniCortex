using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class TypeTextUseCaseTest
    {
        private static TypeTextUseCase CreateUseCase(FakePlayerLoopDispatcher playerLoopDispatcher,
            SpyInputOperations ops)
        {
            return new TypeTextUseCase(
                new PlayerLoopRunner(new FakeMainThreadDispatcher(), playerLoopDispatcher), ops);
        }

        [Test]
        public void ExecuteAsync_TypesTextInFirstFrame_AndWaitsOneMoreFrame()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = CreateUseCase(playerLoopDispatcher, ops);
            var textCountPerFrame = new List<int>();
            playerLoopDispatcher.OnFrame = _ => textCountPerFrame.Add(ops.TextEventHistory.Count);

            // Act
            useCase.ExecuteAsync("Hello あ", CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(2, playerLoopDispatcher.FrameCount);
            CollectionAssert.AreEqual(new[] { 0, 1 }, textCountPerFrame);
            Assert.AreEqual("Hello あ", ops.TextEventHistory.Single().Text);
        }

        [Test]
        public void ExecuteAsync_Throws_WhenTextIsEmpty()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = CreateUseCase(playerLoopDispatcher, ops);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                useCase.ExecuteAsync("", CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(TypeTextUseCase.TextRequiredMessage, ex.Message);
            Assert.AreEqual(0, playerLoopDispatcher.FrameCount);
            CollectionAssert.IsEmpty(ops.TextEventHistory);
        }

        [Test]
        public void ExecuteAsync_BlocksPhysicalKeyboard_UntilTextIsProcessed()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations();
            var useCase = CreateUseCase(playerLoopDispatcher, ops);
            var blockedInLastFrame = false;

            // Act
            playerLoopDispatcher.OnFrame = _ => blockedInLastFrame = ops.PhysicalKeyboardBlockCount > 0;
            useCase.ExecuteAsync("a", CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.IsTrue(ops.TextEventHistory.Single().PhysicalKeyboardBlocked);
            Assert.IsTrue(blockedInLastFrame);
            Assert.AreEqual(0, ops.PhysicalKeyboardBlockCount);
        }

        [Test]
        public void ExecuteAsync_UnblocksPhysicalKeyboard_WhenTypingFails()
        {
            // Arrange
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var ops = new SpyInputOperations { ExceptionOnTypeText = new InvalidOperationException("failed") };
            var useCase = CreateUseCase(playerLoopDispatcher, ops);

            // Act
            Assert.Throws<InvalidOperationException>(() =>
                useCase.ExecuteAsync("a", CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            Assert.AreEqual(0, ops.PhysicalKeyboardBlockCount);
        }
    }
}
