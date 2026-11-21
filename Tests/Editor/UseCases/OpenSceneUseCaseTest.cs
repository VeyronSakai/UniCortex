using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class OpenSceneUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsOpenScene_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication();
            var useCase = new OpenSceneUseCase(dispatcher, sceneManager, editorApp);

            useCase.ExecuteAsync("Assets/Scenes/Main.unity", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, sceneManager.OpenSceneCallCount);
            Assert.AreEqual("Assets/Scenes/Main.unity", sceneManager.LastScenePath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_ThrowsPlayModeException_WhenInPlayMode()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication { IsPlaying = true };
            var useCase = new OpenSceneUseCase(dispatcher, sceneManager, editorApp);

            Assert.Throws<PlayModeException>(() =>
                useCase.ExecuteAsync("Assets/Scenes/Main.unity", CancellationToken.None)
                    .GetAwaiter().GetResult());
            Assert.AreEqual(0, sceneManager.OpenSceneCallCount);
        }

        [Test]
        public void ExecuteAsync_ThrowsUnsavedSceneChangesException_WhenSceneHasUnsavedChanges()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager
            {
                DirtyScenePaths = new List<string> { "Assets/Scenes/Main.unity" }
            };
            var editorApp = new SpyEditorApplication();
            var useCase = new OpenSceneUseCase(dispatcher, sceneManager, editorApp);

            // Act
            var ex = Assert.Throws<UnsavedSceneChangesException>(() =>
                useCase.ExecuteAsync("Assets/Scenes/Other.unity", CancellationToken.None)
                    .GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("Cannot open a scene", ex.Message);
            StringAssert.Contains("Assets/Scenes/Main.unity", ex.Message);
            Assert.AreEqual(0, sceneManager.OpenSceneCallCount);
            Assert.AreEqual(0, editorApp.SaveCallCount);
        }

        [Test]
        public void ExecuteAsync_ThrowsUnsavedPrefabChangesException_WhenPrefabModeHasUnsavedChanges()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager { DirtyPrefabStagePath = "Assets/Prefabs/Open.prefab" };
            var useCase = new OpenSceneUseCase(dispatcher, sceneManager, new SpyEditorApplication());

            // Act
            var ex = Assert.Throws<UnsavedPrefabChangesException>(() =>
                useCase.ExecuteAsync("Assets/Scenes/Other.unity", CancellationToken.None)
                    .GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("Cannot open a scene", ex.Message);
            StringAssert.Contains("Assets/Prefabs/Open.prefab", ex.Message);
            Assert.AreEqual(0, sceneManager.OpenSceneCallCount);
        }
    }
}
