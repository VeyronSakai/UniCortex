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
    internal sealed class SetAssetPropertyHandlerTest
    {
        [Test]
        public void HandleSetProperty_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyAssetOperations();
            var useCase = new SetAssetPropertyUseCase(dispatcher, operations);
            var handler = new SetAssetPropertyHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.AssetSetProperty,
                Body = "{\"assetPath\":\"Assets/Test.mat\",\"propertyPath\":\"m_Name\",\"value\":\"NewName\"}"
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual("m_Name", operations.LastSetPropertyPath);
        }

        [Test]
        public void HandleSetProperty_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyAssetOperations();
            var useCase = new SetAssetPropertyUseCase(dispatcher, operations);
            var handler = new SetAssetPropertyHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.AssetSetProperty,
                Body = ""
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("assetPath, propertyPath, and value are required", context.ResponseBody);
        }

        [Test]
        public void HandleSetProperty_Returns400_WhenPropertyPathMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyAssetOperations();
            var useCase = new SetAssetPropertyUseCase(dispatcher, operations);
            var handler = new SetAssetPropertyHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "POST",
                Path = ApiRoutes.AssetSetProperty,
                Body = "{\"assetPath\":\"Assets/Test.mat\"}"
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("propertyPath is required", context.ResponseBody);
        }
    }
}
