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
    internal sealed class AssetRefreshHandlerTest
    {
        [Test]
        public void HandleRefresh_Returns200WithSuccess()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyAssetDatabaseOperations();
            var useCase = new RefreshAssetDatabaseUseCase(dispatcher, operations);
            var handler = new AssetRefreshHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.AssetRefresh);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(1, operations.RefreshCallCount);
        }
    }
}
