using System;
using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SaveSceneAsUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSaveActiveSceneAs_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication();
            var useCase = new SaveSceneAsUseCase(dispatcher, sceneManager, editorApp);

            // Act
            useCase.ExecuteAsync("Assets/Scenes/New.unity", CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, sceneManager.SaveActiveSceneAsCallCount);
            Assert.AreEqual("Assets/Scenes/New.unity", sceneManager.LastSaveAsScenePath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_ThrowsPlayModeException_WhenInPlayMode()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication { IsPlaying = true };
            var useCase = new SaveSceneAsUseCase(dispatcher, sceneManager, editorApp);

            // Act
            var ex = Assert.Throws<PlayModeException>(() =>
                useCase.ExecuteAsync("Assets/Scenes/New.unity", CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("Cannot save a scene during play mode", ex.Message);
            Assert.AreEqual(0, sceneManager.SaveActiveSceneAsCallCount);
        }

        [Test]
        public void ExecuteAsync_ThrowsArgumentException_WhenPathIsNotSceneFile()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication();
            var useCase = new SaveSceneAsUseCase(dispatcher, sceneManager, editorApp);

            // Act
            var ex = Assert.Throws<ArgumentException>(() =>
                useCase.ExecuteAsync("Assets/Scenes/New.prefab", CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("must end with '.unity'", ex.Message);
            Assert.AreEqual(0, sceneManager.SaveActiveSceneAsCallCount);
            Assert.AreEqual(0, dispatcher.CallCount);
        }
    }
}
