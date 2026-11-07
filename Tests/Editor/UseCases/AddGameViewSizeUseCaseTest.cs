using System;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class AddGameViewSizeUseCaseTest
    {
        private FakeMainThreadDispatcher _dispatcher;
        private SpyEditorWindowOperations _operations;
        private AddGameViewSizeUseCase _useCase;

        [SetUp]
        public void SetUp()
        {
            _dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyEditorWindowOperations
            {
                SizeListEntries = new[]
                {
                    new GameViewSizeEntry
                    {
                        index = 0, name = "Free Aspect", width = 0, height = 0, sizeType = "AspectRatio"
                    },
                    new GameViewSizeEntry
                    {
                        index = 1, name = "Phone Half", width = 642, height = 1389, sizeType = "FixedResolution"
                    },
                    new GameViewSizeEntry
                    {
                        index = 2, name = "Phone Aspect", width = 642, height = 1389, sizeType = "AspectRatio"
                    }
                }
            };
            _useCase = new AddGameViewSizeUseCase(_dispatcher, _operations);
        }

        [Test]
        public void ExecuteAsync_AddsSize_WithoutSelectingIt_WhenNoSameSizeExists()
        {
            // Act
            var result = _useCase.ExecuteAsync("Phone Quarter", 321, 694, CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(3, result.index);
            Assert.IsTrue(result.added);
            Assert.AreEqual(1, _operations.AddGameViewSizeCallCount);
            Assert.AreEqual(("Phone Quarter", 321, 694), _operations.LastAddedSize);
            Assert.AreEqual(0, _operations.SetGameViewSizeCallCount);
            Assert.AreEqual(1, _dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_ReturnsExistingIndex_WithoutAdding_WhenSameNameAndSizeExists()
        {
            // Act
            var result = _useCase.ExecuteAsync("Phone Half", 642, 1389, CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, result.index);
            Assert.IsFalse(result.added);
            Assert.AreEqual(0, _operations.AddGameViewSizeCallCount);
        }

        [TestCase("Phone Half", 642, 1390)]
        [TestCase("Phone Half", 1389, 642)]
        [TestCase("phone half", 642, 1389)]
        [TestCase("Phone Aspect", 642, 1389)]
        public void ExecuteAsync_AddsSize_WhenNameOrSizeDiffers_OrOnlyAnAspectRatioMatches(
            string name, int width, int height)
        {
            // Act
            var result = _useCase.ExecuteAsync(name, width, height, CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(3, result.index);
            Assert.IsTrue(result.added);
            Assert.AreEqual(1, _operations.AddGameViewSizeCallCount);
        }

        [Test]
        public void ExecuteAsync_AcceptsBoundaryValues()
        {
            // Arrange
            var name = new string('a', AddGameViewSizeUseCase.MaxNameLength);

            // Act
            var result = _useCase.ExecuteAsync(name, AddGameViewSizeUseCase.MinResolution,
                AddGameViewSizeUseCase.MaxResolution, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.IsTrue(result.added);
            Assert.AreEqual((name, AddGameViewSizeUseCase.MinResolution, AddGameViewSizeUseCase.MaxResolution),
                _operations.LastAddedSize);
        }

        [TestCase(null, 642, 1389)]
        [TestCase("", 642, 1389)]
        [TestCase("   ", 642, 1389)]
        [TestCase("Phone Half", 9, 1389)]
        [TestCase("Phone Half", 642, 0)]
        [TestCase("Phone Half", -642, 1389)]
        [TestCase("Phone Half", 100000, 1389)]
        [TestCase("Phone Half", 642, 100000)]
        public void ExecuteAsync_Throws_WithoutDispatching_WhenArgumentsAreInvalid(string name, int width, int height)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _useCase.ExecuteAsync(name, width, height, CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(0, _dispatcher.CallCount);
            Assert.AreEqual(0, _operations.AddGameViewSizeCallCount);
        }

        [Test]
        public void ExecuteAsync_Throws_WithoutDispatching_WhenNameIsTooLong()
        {
            // Arrange
            var name = new string('a', AddGameViewSizeUseCase.MaxNameLength + 1);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _useCase.ExecuteAsync(name, 642, 1389, CancellationToken.None).GetAwaiter().GetResult());
            StringAssert.Contains($"{AddGameViewSizeUseCase.MaxNameLength} characters", exception.Message);
            Assert.AreEqual(0, _dispatcher.CallCount);
        }
    }
}
