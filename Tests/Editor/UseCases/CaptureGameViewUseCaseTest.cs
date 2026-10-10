using System;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class CaptureGameViewUseCaseTest
    {
        [Test]
        public void ExecuteAsync_OpensGameView_ThenReturnsData_WhenPlaying()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var editorApplication = new SpyEditorApplication { IsPlaying = true };
            var windowOperations = new SpyEditorWindowOperations();
            var captureOperations = new SpyCaptureOperations
            {
                GameViewResult = new byte[] { 0x89, 0x50, 0x4E, 0x47 }
            };
            var useCase = new CaptureGameViewUseCase(dispatcher, editorApplication, windowOperations,
                captureOperations, new SpyPlayModeViewOperations());

            // Act
            var result = useCase.ExecuteAsync(false, false, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, windowOperations.OpenGameViewCallCount);
            Assert.AreEqual(1, captureOperations.CaptureGameViewCallCount);
            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(0x89, result[0]);
            Assert.AreEqual(2, dispatcher.CallCount);
            Assert.IsNull(captureOperations.LastSafeAreaToDraw);
        }

        [Test]
        public void ExecuteAsync_PassesSafeArea_WhenDrawSafeArea()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var editorApplication = new SpyEditorApplication { IsPlaying = true };
            var captureOperations = new SpyCaptureOperations();
            var playModeViewOperations = new SpyPlayModeViewOperations();
            var useCase = new CaptureGameViewUseCase(dispatcher, editorApplication,
                new SpyEditorWindowOperations(), captureOperations, playModeViewOperations);

            // Act
            useCase.ExecuteAsync(true, false, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreSame(playModeViewOperations.SafeArea, captureOperations.LastSafeAreaToDraw);
            Assert.AreEqual(1, playModeViewOperations.GetScreenSafeAreaCallCount);
        }

        [Test]
        public void ExecuteAsync_Throws_WithoutOpeningOrCapturing_WhenNotPlaying()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var editorApplication = new SpyEditorApplication { IsPlaying = false };
            var windowOperations = new SpyEditorWindowOperations();
            var captureOperations = new SpyCaptureOperations();
            var useCase = new CaptureGameViewUseCase(dispatcher, editorApplication, windowOperations,
                captureOperations, new SpyPlayModeViewOperations());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                useCase.ExecuteAsync(false, false, CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(0, windowOperations.OpenGameViewCallCount);
            Assert.AreEqual(0, captureOperations.CaptureGameViewCallCount);
        }

        [Test]
        public void ExecuteAsync_PassesDeviceFrame_WhenSimulatorView()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var editorApplication = new SpyEditorApplication { IsPlaying = true };
            var captureOperations = new SpyCaptureOperations();
            var playModeViewOperations = new SpyPlayModeViewOperations { ViewType = PlayModeViewTypes.SimulatorView };
            var useCase = new CaptureGameViewUseCase(dispatcher, editorApplication,
                new SpyEditorWindowOperations(), captureOperations, playModeViewOperations);

            // Act
            useCase.ExecuteAsync(false, true, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.IsTrue(captureOperations.LastDrawDeviceFrame);
            Assert.AreEqual(1, captureOperations.CaptureGameViewCallCount);
        }

        [Test]
        public void ExecuteAsync_DrawsDeviceFrame_WhenSimulatorViewAndDeviceFrameIsOmitted()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var editorApplication = new SpyEditorApplication { IsPlaying = true };
            var captureOperations = new SpyCaptureOperations();
            var playModeViewOperations = new SpyPlayModeViewOperations { ViewType = PlayModeViewTypes.SimulatorView };
            var useCase = new CaptureGameViewUseCase(dispatcher, editorApplication,
                new SpyEditorWindowOperations(), captureOperations, playModeViewOperations);

            // Act
            useCase.ExecuteAsync(false, null, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.IsTrue(captureOperations.LastDrawDeviceFrame);
        }

        [Test]
        public void ExecuteAsync_DoesNotDrawDeviceFrame_WhenGameViewAndDeviceFrameIsOmitted()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var editorApplication = new SpyEditorApplication { IsPlaying = true };
            var captureOperations = new SpyCaptureOperations();
            var playModeViewOperations = new SpyPlayModeViewOperations { ViewType = PlayModeViewTypes.GameView };
            var useCase = new CaptureGameViewUseCase(dispatcher, editorApplication,
                new SpyEditorWindowOperations(), captureOperations, playModeViewOperations);

            // Act
            useCase.ExecuteAsync(false, null, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.IsFalse(captureOperations.LastDrawDeviceFrame);
            Assert.AreEqual(1, captureOperations.CaptureGameViewCallCount);
        }

        [Test]
        public void ExecuteAsync_Throws_WithoutCapturing_WhenGameViewAndDeviceFrameIsTrue()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var editorApplication = new SpyEditorApplication { IsPlaying = true };
            var windowOperations = new SpyEditorWindowOperations();
            var captureOperations = new SpyCaptureOperations();
            var playModeViewOperations = new SpyPlayModeViewOperations { ViewType = PlayModeViewTypes.GameView };
            var useCase = new CaptureGameViewUseCase(dispatcher, editorApplication, windowOperations,
                captureOperations, playModeViewOperations);

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() =>
                useCase.ExecuteAsync(false, true, CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("Simulator view", ex.Message);
            Assert.AreEqual(0, windowOperations.OpenGameViewCallCount);
            Assert.AreEqual(0, captureOperations.CaptureGameViewCallCount);
        }
    }
}
