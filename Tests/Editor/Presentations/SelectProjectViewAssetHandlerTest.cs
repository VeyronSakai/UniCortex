using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.ProjectView;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class SelectProjectViewAssetHandlerTest
    {
        [Test]
        public void HandleSelect_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyProjectViewOperations();
            var useCase = new SelectProjectViewAssetUseCase(dispatcher, operations);
            var handler = new SelectProjectViewAssetHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ProjectViewSelect,
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
            var operations = new SpyProjectViewOperations();
            var useCase = new SelectProjectViewAssetUseCase(dispatcher, operations);
            var handler = new SelectProjectViewAssetHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ProjectViewSelect);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("assetPath is required", context.ResponseBody);
        }

        [Test]
        public void HandleSelect_Returns400_WhenAssetPathMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyProjectViewOperations();
            var useCase = new SelectProjectViewAssetUseCase(dispatcher, operations);
            var handler = new SelectProjectViewAssetHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ProjectViewSelect, "{}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("assetPath is required", context.ResponseBody);
        }
    }
}
