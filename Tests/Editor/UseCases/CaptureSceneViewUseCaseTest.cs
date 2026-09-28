using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class CaptureSceneViewUseCaseTest
    {
        [Test]
        public void ExecuteAsync_FocusesSceneView_ThenReturnsData()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyCaptureOperations
            {
                SceneViewResult = new byte[] { 0x89, 0x50, 0x4E, 0x47 }
            };
            var windowOperations = new SpyEditorWindowOperations();
            var useCase = new CaptureSceneViewUseCase(dispatcher, windowOperations, operations);

            // Act
            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, windowOperations.FocusSceneViewCallCount);
            Assert.AreEqual(1, operations.CaptureSceneViewCallCount);
            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(0x89, result[0]);
            Assert.AreEqual(2, dispatcher.CallCount);
        }
    }
}
