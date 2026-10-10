using System;
using System.Threading;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SwitchPlatformUseCaseTest
    {
        [Test]
        public void ExecuteAsync_SwitchesBuildTarget_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyBuildTargetOperations { ActiveBuildTarget = "StandaloneOSX" };
            var useCase = new SwitchPlatformUseCase(dispatcher, operations, new SpyEditorApplication());

            // Act
            var result = useCase.ExecuteAsync("Android", CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual("StandaloneOSX", result.previousBuildTarget);
            Assert.AreEqual("Android", result.activeBuildTarget);
            CollectionAssert.AreEqual(new[] { "Android" }, operations.SwitchedBuildTargets);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_ResolvesBuildTargetNameCaseInsensitively()
        {
            // Arrange
            var operations = new SpyBuildTargetOperations { ActiveBuildTarget = "StandaloneOSX" };
            var useCase = new SwitchPlatformUseCase(new FakeMainThreadDispatcher(), operations,
                new SpyEditorApplication());

            // Act
            var result = useCase.ExecuteAsync("android", CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual("Android", result.activeBuildTarget);
            CollectionAssert.AreEqual(new[] { "Android" }, operations.SwitchedBuildTargets);
        }

        [Test]
        public void ExecuteAsync_DoesNotSwitch_WhenBuildTargetIsAlreadyActive()
        {
            // Arrange
            var operations = new SpyBuildTargetOperations { ActiveBuildTarget = "Android" };
            var useCase = new SwitchPlatformUseCase(new FakeMainThreadDispatcher(), operations,
                new SpyEditorApplication());

            // Act
            var result = useCase.ExecuteAsync("Android", CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual("Android", result.previousBuildTarget);
            Assert.AreEqual("Android", result.activeBuildTarget);
            Assert.IsEmpty(operations.SwitchedBuildTargets);
        }

        [Test]
        public void ExecuteAsync_ThrowsArgumentException_WhenBuildTargetIsUnknown()
        {
            // Arrange
            var operations = new SpyBuildTargetOperations();
            var useCase = new SwitchPlatformUseCase(new FakeMainThreadDispatcher(), operations,
                new SpyEditorApplication());

            // Act
            var ex = Assert.Throws<ArgumentException>(() =>
                useCase.ExecuteAsync("Unknown", CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("Unknown build target", ex.Message);
            StringAssert.Contains("StandaloneOSX, Android", ex.Message);
            Assert.IsEmpty(operations.SwitchedBuildTargets);
        }

        [Test]
        public void ExecuteAsync_ThrowsArgumentException_WhenBuildTargetIsNotSupported()
        {
            // Arrange
            var operations = new SpyBuildTargetOperations();
            var useCase = new SwitchPlatformUseCase(new FakeMainThreadDispatcher(), operations,
                new SpyEditorApplication());

            // Act
            var ex = Assert.Throws<ArgumentException>(() =>
                useCase.ExecuteAsync("iOS", CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("not supported", ex.Message);
            Assert.IsEmpty(operations.SwitchedBuildTargets);
        }

        [Test]
        public void ExecuteAsync_ThrowsInvalidOperationException_WhenSwitchFails()
        {
            // Arrange
            var operations = new SpyBuildTargetOperations { SwitchSucceeds = false };
            var useCase = new SwitchPlatformUseCase(new FakeMainThreadDispatcher(), operations,
                new SpyEditorApplication());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                useCase.ExecuteAsync("Android", CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void ExecuteAsync_ThrowsPlayModeException_WhenInPlayMode()
        {
            // Arrange
            var operations = new SpyBuildTargetOperations();
            var useCase = new SwitchPlatformUseCase(new FakeMainThreadDispatcher(), operations,
                new SpyEditorApplication { IsPlaying = true });

            // Act & Assert
            Assert.Throws<PlayModeException>(() =>
                useCase.ExecuteAsync("Android", CancellationToken.None).GetAwaiter().GetResult());
            Assert.IsEmpty(operations.SwitchedBuildTargets);
        }
    }
}
