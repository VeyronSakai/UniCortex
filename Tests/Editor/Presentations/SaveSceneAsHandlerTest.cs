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
    internal sealed class SaveSceneAsHandlerTest
    {
        [Test]
        public void HandleSaveSceneAs_Returns200_WhenScenePathProvided()
        {
            // Arrange
            var sceneManager = new SpyEditorSceneManager();
            var router = CreateRouter(sceneManager, new SpyEditorApplication());
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SceneSaveAs,
                "{\"scenePath\":\"Assets/Scenes/New.unity\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual("Assets/Scenes/New.unity", sceneManager.LastSaveAsScenePath);
        }

        [TestCase("{}")]
        [TestCase("")]
        public void HandleSaveSceneAs_Returns400_WhenScenePathMissing(string body)
        {
            // Arrange
            var sceneManager = new SpyEditorSceneManager();
            var router = CreateRouter(sceneManager, new SpyEditorApplication());
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SceneSaveAs, body);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("scenePath is required", context.ResponseBody);
            Assert.AreEqual(0, sceneManager.SaveActiveSceneAsCallCount);
        }

        [Test]
        public void HandleSaveSceneAs_Returns400_WhenPathIsNotSceneFile()
        {
            // Arrange
            var sceneManager = new SpyEditorSceneManager();
            var router = CreateRouter(sceneManager, new SpyEditorApplication());
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SceneSaveAs,
                "{\"scenePath\":\"Assets/Scenes/New\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("must end with '.unity'", context.ResponseBody);
            Assert.AreEqual(0, sceneManager.SaveActiveSceneAsCallCount);
        }

        [Test]
        public void HandleSaveSceneAs_Returns400_WhenInPlayMode()
        {
            // Arrange
            var sceneManager = new SpyEditorSceneManager();
            var router = CreateRouter(sceneManager, new SpyEditorApplication { IsPlaying = true });
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SceneSaveAs,
                "{\"scenePath\":\"Assets/Scenes/New.unity\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Cannot save a scene during play mode", context.ResponseBody);
            Assert.AreEqual(0, sceneManager.SaveActiveSceneAsCallCount);
        }

        private static RequestRouter CreateRouter(SpyEditorSceneManager sceneManager,
            SpyEditorApplication editorApplication)
        {
            var useCase = new SaveSceneAsUseCase(new FakeMainThreadDispatcher(), sceneManager, editorApplication);
            var handler = new SaveSceneAsHandler(useCase);
            var router = new RequestRouter();
            handler.Register(router);
            return router;
        }
    }
}
