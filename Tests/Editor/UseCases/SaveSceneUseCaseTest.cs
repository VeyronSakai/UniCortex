using System;
using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SaveSceneUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSaveOpenScenes_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication();
            var useCase = new SaveSceneUseCase(dispatcher, sceneManager, editorApp);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(result);
            Assert.AreEqual(1, sceneManager.SaveOpenScenesCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_ThrowsInvalidOperationException_WhenInPlayMode()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication { IsPlaying = true };
            var useCase = new SaveSceneUseCase(dispatcher, sceneManager, editorApp);

            Assert.Throws<InvalidOperationException>(() =>
                useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(0, sceneManager.SaveOpenScenesCallCount);
        }
    }
}
