using System;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetSimulatorDeviceUseCaseTest
    {
        private FakeMainThreadDispatcher _dispatcher;
        private SpyPlayModeViewOperations _operations;
        private SetSimulatorDeviceUseCase _useCase;

        [SetUp]
        public void SetUp()
        {
            _dispatcher = new FakeMainThreadDispatcher();
            _operations = new SpyPlayModeViewOperations { ViewType = PlayModeViewTypes.SimulatorView };
            _useCase = new SetSimulatorDeviceUseCase(_dispatcher, _operations);
        }

        [Test]
        public void ExecuteAsync_SetsDeviceAndRotation()
        {
            // Act
            var result = _useCase.ExecuteAsync(1, 270, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual("Google Pixel 5", result.deviceName);
            Assert.AreEqual(270, result.rotation);
            Assert.AreEqual(1, _operations.SetSimulatorDeviceCallCount);
            Assert.AreEqual(1, _operations.SetSimulatorRotationCallCount);
            Assert.AreEqual(1, _dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_KeepsRotation_WhenRotationIsUnchanged()
        {
            // Arrange
            _operations.Rotation = 90;

            // Act
            var result = _useCase.ExecuteAsync(1, SetSimulatorDeviceUseCase.Unchanged, CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual("Google Pixel 5", result.deviceName);
            Assert.AreEqual(90, result.rotation);
            Assert.AreEqual(0, _operations.SetSimulatorRotationCallCount);
        }

        [Test]
        public void ExecuteAsync_KeepsDevice_WhenIndexIsUnchanged()
        {
            // Act
            var result = _useCase.ExecuteAsync(SetSimulatorDeviceUseCase.Unchanged, 180, CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual("Apple iPhone 13", result.deviceName);
            Assert.AreEqual(180, result.rotation);
            Assert.AreEqual(0, _operations.SetSimulatorDeviceCallCount);
        }

        [TestCase(SetSimulatorDeviceUseCase.Unchanged, SetSimulatorDeviceUseCase.Unchanged)]
        [TestCase(-2, SetSimulatorDeviceUseCase.Unchanged)]
        [TestCase(SetSimulatorDeviceUseCase.Unchanged, 45)]
        [TestCase(SetSimulatorDeviceUseCase.Unchanged, 360)]
        public void ExecuteAsync_Throws_WithoutDispatching_WhenArgumentsAreInvalid(int index, int rotation)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _useCase.ExecuteAsync(index, rotation, CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(0, _dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_Throws_WhenIndexIsOutOfRange()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _useCase.ExecuteAsync(5, SetSimulatorDeviceUseCase.Unchanged, CancellationToken.None)
                    .GetAwaiter().GetResult());
        }
    }
}
