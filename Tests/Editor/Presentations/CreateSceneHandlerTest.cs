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
    internal sealed class CreateSceneHandlerTest
    {
        [Test]
        public void HandleCreateScene_Returns200_WhenScenePathProvided()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var useCase = new CreateSceneUseCase(dispatcher, sceneManager);
            var handler = new CreateSceneHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SceneCreate, "{\"scenePath\":\"Assets/Scenes/New.unity\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual("Assets/Scenes/New.unity", sceneManager.LastCreateScenePath);
        }

        [Test]
        public void HandleCreateScene_Returns400_WhenScenePathMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var useCase = new CreateSceneUseCase(dispatcher, sceneManager);
            var handler = new CreateSceneHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SceneCreate, "{}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("scenePath is required", context.ResponseBody);
        }

        [Test]
        public void HandleCreateScene_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var useCase = new CreateSceneUseCase(dispatcher, sceneManager);
            var handler = new CreateSceneHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SceneCreate, "");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("scenePath is required", context.ResponseBody);
        }

        [Test]
        public void HandleCreateScene_Returns500_WhenSceneCreationFails()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager { CreateSceneResult = false };
            var useCase = new CreateSceneUseCase(dispatcher, sceneManager);
            var handler = new CreateSceneHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.SceneCreate,
                "{\"scenePath\":\"Assets/Scenes/New.unity\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.InternalServerError, context.ResponseStatusCode);
            StringAssert.Contains("Failed to create scene", context.ResponseBody);
        }
    }
}
