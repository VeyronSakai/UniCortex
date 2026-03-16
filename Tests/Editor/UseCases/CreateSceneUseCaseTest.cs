using System.Threading;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class CreateSceneUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsCreateScene_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication();
            var useCase = new CreateSceneUseCase(dispatcher, sceneManager, editorApp);

            var result = useCase.ExecuteAsync("Assets/Scenes/New.unity", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.IsTrue(result);
            Assert.AreEqual(1, sceneManager.CreateSceneCallCount);
            Assert.AreEqual("Assets/Scenes/New.unity", sceneManager.LastCreateScenePath);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_ThrowsPlayModeException_WhenInPlayMode()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication { IsPlaying = true };
            var useCase = new CreateSceneUseCase(dispatcher, sceneManager, editorApp);

            Assert.Throws<PlayModeException>(() =>
                useCase.ExecuteAsync("Assets/Scenes/New.unity", CancellationToken.None)
                    .GetAwaiter().GetResult());
            Assert.AreEqual(0, sceneManager.CreateSceneCallCount);
        }
    }
}
