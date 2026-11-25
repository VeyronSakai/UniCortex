using System;
using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class UnpackPrefabInstanceUseCaseTest
    {
        [Test]
        public void ExecuteAsync_PassesInstanceIdAndMode_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var useCase = new UnpackPrefabInstanceUseCase(dispatcher, operations);

            // Act
            useCase.ExecuteAsync(12345, PrefabUnpackModes.Completely, CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, operations.UnpackCallCount);
            Assert.AreEqual(12345, operations.LastUnpackInstanceId);
            Assert.AreEqual(PrefabUnpackModes.Completely, operations.LastUnpackMode);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [TestCase(null)]
        [TestCase("")]
        public void ExecuteAsync_WithoutMode_UsesOutermostRoot(string mode)
        {
            // Arrange
            var operations = new SpyPrefabOperations();
            var useCase = new UnpackPrefabInstanceUseCase(new FakeMainThreadDispatcher(), operations);

            // Act
            useCase.ExecuteAsync(12345, mode, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(PrefabUnpackModes.OutermostRoot, operations.LastUnpackMode);
        }

        [Test]
        public void ExecuteAsync_WithUnknownMode_ThrowsArgumentException_WithoutDispatching()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var useCase = new UnpackPrefabInstanceUseCase(dispatcher, operations);

            // Act
            var ex = Assert.Throws<ArgumentException>(() =>
                useCase.ExecuteAsync(12345, "completely", CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("Unknown unpack mode: 'completely'", ex.Message);
            StringAssert.Contains("OutermostRoot, Completely", ex.Message);
            Assert.AreEqual(0, operations.UnpackCallCount);
            Assert.AreEqual(0, dispatcher.CallCount);
        }
    }
}
