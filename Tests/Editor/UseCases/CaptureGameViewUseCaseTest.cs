using System;
using System.Threading;
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
                captureOperations);

            // Act
            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, windowOperations.OpenGameViewCallCount);
            Assert.AreEqual(1, captureOperations.CaptureGameViewCallCount);
            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(0x89, result[0]);
            Assert.AreEqual(2, dispatcher.CallCount);
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
                captureOperations);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(0, windowOperations.OpenGameViewCallCount);
            Assert.AreEqual(0, captureOperations.CaptureGameViewCallCount);
        }
    }
}
