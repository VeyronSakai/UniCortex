using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Scene;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class SaveSceneHandlerTest
    {
        [Test]
        public void HandleSaveScene_Returns200WithSuccess()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication();
            var useCase = new SaveSceneUseCase(dispatcher, sceneManager, editorApp);
            var handler = new SaveSceneHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SceneSave);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(1, sceneManager.SaveOpenScenesCallCount);
        }

        [Test]
        public void HandleSaveScene_Returns400_WhenInPlayMode()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication { IsPlaying = true };
            var useCase = new SaveSceneUseCase(dispatcher, sceneManager, editorApp);
            var handler = new SaveSceneHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SceneSave);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Cannot save scene during play mode", context.ResponseBody);
            Assert.AreEqual(0, sceneManager.SaveOpenScenesCallCount);
        }
    }
}
