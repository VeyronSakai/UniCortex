using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Asset;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class CreateAssetHandlerTest
    {
        [Test]
        public void HandleCreate_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyAssetOperations();
            var useCase = new CreateAssetUseCase(dispatcher, operations);
            var handler = new CreateAssetHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.AssetCreate,
                Body = "{\"type\":\"Material\",\"assetPath\":\"Assets/Materials/Test.mat\"}"
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual("Material", operations.LastCreateType);
        }

        [Test]
        public void HandleCreate_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyAssetOperations();
            var useCase = new CreateAssetUseCase(dispatcher, operations);
            var handler = new CreateAssetHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.AssetCreate,
                Body = ""
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("type and assetPath are required", context.ResponseBody);
        }

        [Test]
        public void HandleCreate_Returns400_WhenTypeMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyAssetOperations();
            var useCase = new CreateAssetUseCase(dispatcher, operations);
            var handler = new CreateAssetHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.AssetCreate,
                Body = "{\"assetPath\":\"Assets/Materials/Test.mat\"}"
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("type is required", context.ResponseBody);
        }
    }
}
