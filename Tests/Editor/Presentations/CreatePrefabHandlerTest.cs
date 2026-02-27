using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Prefab;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class CreatePrefabHandlerTest
    {
        [Test]
        public void HandleCreate_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var useCase = new CreatePrefabUseCase(dispatcher, operations);
            var handler = new CreatePrefabHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.PrefabCreate,
                Body = "{\"instanceId\":12345,\"assetPath\":\"Assets/Prefabs/Test.prefab\"}"
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(12345, operations.LastCreateInstanceId);
            Assert.AreEqual("Assets/Prefabs/Test.prefab", operations.LastCreateAssetPath);
        }

        [Test]
        public void HandleCreate_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var useCase = new CreatePrefabUseCase(dispatcher, operations);
            var handler = new CreatePrefabHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.PrefabCreate,
                Body = ""
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("instanceId and assetPath are required", context.ResponseBody);
        }

        [Test]
        public void HandleCreate_Returns400_WhenAssetPathMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var useCase = new CreatePrefabUseCase(dispatcher, operations);
            var handler = new CreatePrefabHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.PrefabCreate,
                Body = "{\"instanceId\":12345}"
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("assetPath is required", context.ResponseBody);
        }
    }
}
