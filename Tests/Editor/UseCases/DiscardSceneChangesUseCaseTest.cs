using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class DiscardSceneChangesUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsDiscardChanges_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication();
            var useCase = new DiscardSceneChangesUseCase(dispatcher, sceneManager, editorApp);

            // Act
            useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, sceneManager.DiscardChangesCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_ThrowsPlayModeException_WhenInPlayMode()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication { IsPlaying = true };
            var useCase = new DiscardSceneChangesUseCase(dispatcher, sceneManager, editorApp);

            // Act
            var ex = Assert.Throws<PlayModeException>(() =>
                useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("Cannot discard scene changes during play mode", ex.Message);
            Assert.AreEqual(0, sceneManager.DiscardChangesCallCount);
        }
    }
}
