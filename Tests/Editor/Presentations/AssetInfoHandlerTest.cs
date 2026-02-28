using System.Collections.Generic;
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
    internal sealed class AssetInfoHandlerTest
    {
        [Test]
        public void HandleInfo_Returns200_WithAssetInfo()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyAssetOperations
            {
                GetInfoResult = new AssetInfoResponse("Assets/Test.mat", "Material",
                    new List<SerializedPropertyEntry>
                    {
                        new SerializedPropertyEntry("m_Name", "String", "Test")
                    })
            };
            var useCase = new GetAssetInfoUseCase(dispatcher, operations);
            var handler = new AssetInfoHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.AssetInfo);
            context.SetQueryParameter("assetPath", "Assets/Test.mat");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("Material", context.ResponseBody);
            StringAssert.Contains("m_Name", context.ResponseBody);
        }

        [Test]
        public void HandleInfo_Returns400_WhenAssetPathMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyAssetOperations();
            var useCase = new GetAssetInfoUseCase(dispatcher, operations);
            var handler = new AssetInfoHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.AssetInfo);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("assetPath query parameter is required", context.ResponseBody);
        }
    }
}
