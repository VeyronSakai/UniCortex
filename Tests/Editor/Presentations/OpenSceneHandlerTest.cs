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
    internal sealed class OpenSceneHandlerTest
    {
        [Test]
        public void HandleOpenScene_Returns200_WhenScenePathProvided()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var useCase = new OpenSceneUseCase(dispatcher, sceneManager);
            var handler = new OpenSceneHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.SceneOpen,
                Body = "{\"scenePath\":\"Assets/Scenes/Main.unity\"}"
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual("Assets/Scenes/Main.unity", sceneManager.LastScenePath);
        }

        [Test]
        public void HandleOpenScene_Returns400_WhenScenePathMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var useCase = new OpenSceneUseCase(dispatcher, sceneManager);
            var handler = new OpenSceneHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.SceneOpen,
                Body = "{}"
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("scenePath is required", context.ResponseBody);
        }

        [Test]
        public void HandleOpenScene_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var sceneManager = new SpyEditorSceneManager();
            var useCase = new OpenSceneUseCase(dispatcher, sceneManager);
            var handler = new OpenSceneHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.SceneOpen,
                Body = ""
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("scenePath is required", context.ResponseBody);
        }
    }
}
