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
    internal sealed class InstantiatePrefabHandlerTest
    {
        [Test]
        public void HandleInstantiate_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations
            {
                InstantiateResult = new InstantiatePrefabResponse("MyCube", 56789)
            };
            var useCase = new InstantiatePrefabUseCase(dispatcher, operations);
            var handler = new InstantiatePrefabHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.PrefabInstantiate,
                "{\"assetPath\":\"Assets/Prefabs/MyCube.prefab\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("MyCube", context.ResponseBody);
            StringAssert.Contains("56789", context.ResponseBody);
        }

        [Test]
        public void HandleInstantiate_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var useCase = new InstantiatePrefabUseCase(dispatcher, operations);
            var handler = new InstantiatePrefabHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.PrefabInstantiate);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("assetPath is required", context.ResponseBody);
        }

        [Test]
        public void HandleInstantiate_Returns400_WhenAssetPathMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPrefabOperations();
            var useCase = new InstantiatePrefabUseCase(dispatcher, operations);
            var handler = new InstantiatePrefabHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.PrefabInstantiate, "{}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("assetPath is required", context.ResponseBody);
        }
    }
}
