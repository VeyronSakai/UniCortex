using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Scene;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class DiscardSceneChangesHandlerTest
    {
        [Test]
        public void HandleDiscardSceneChanges_Returns200()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication();
            var useCase = new DiscardSceneChangesUseCase(dispatcher, sceneManager, editorApp);
            var handler = new DiscardSceneChangesHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SceneDiscard);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(1, sceneManager.DiscardChangesCallCount);
        }

        [Test]
        public void HandleDiscardSceneChanges_Returns400_WhenInPlayMode()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var editorApp = new SpyEditorApplication { IsPlaying = true };
            var useCase = new DiscardSceneChangesUseCase(dispatcher, sceneManager, editorApp);
            var handler = new DiscardSceneChangesHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SceneDiscard);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Cannot discard scene changes during play mode", context.ResponseBody);
            Assert.AreEqual(0, sceneManager.DiscardChangesCallCount);
        }
    }
}
