using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.ProjectWindow;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class SelectProjectWindowAssetHandlerTest
    {
        [Test]
        public void HandleSelect_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyProjectWindowOperations();
            var useCase = new SelectProjectWindowAssetUseCase(dispatcher, operations);
            var handler = new SelectProjectWindowAssetHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ProjectWindowSelect,
                "{\"assetPath\":\"Assets/Scenes/SampleScene.unity\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(1, operations.SelectAssetCallCount);
            Assert.AreEqual("Assets/Scenes/SampleScene.unity", operations.LastSelectedAssetPath);
        }

        [Test]
        public void HandleSelect_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyProjectWindowOperations();
            var useCase = new SelectProjectWindowAssetUseCase(dispatcher, operations);
            var handler = new SelectProjectWindowAssetHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ProjectWindowSelect);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("assetPath is required", context.ResponseBody);
        }

        [Test]
        public void HandleSelect_Returns400_WhenAssetPathMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyProjectWindowOperations();
            var useCase = new SelectProjectWindowAssetUseCase(dispatcher, operations);
            var handler = new SelectProjectWindowAssetHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ProjectWindowSelect, "{}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("assetPath is required", context.ResponseBody);
        }
    }
}
