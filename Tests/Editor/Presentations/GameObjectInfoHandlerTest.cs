using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.GameObject;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class GameObjectInfoHandlerTest
    {
        [Test]
        public void HandleInfo_Returns200_WithGameObjectInfo()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            ops.GetInfoResult = new GameObjectInfoResponse("Cube", 123, true, "Untagged", 0,
                new List<ComponentInfoEntry> { new ComponentInfoEntry("Transform", 0) });
            var useCase = new GetGameObjectInfoUseCase(dispatcher, ops);
            var handler = new GameObjectInfoHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.GameObjectInfo);
            context.SetQueryParameter("instanceId", "123");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("Cube", context.ResponseBody);
            StringAssert.Contains("Transform", context.ResponseBody);
        }

        [Test]
        public void HandleInfo_Returns400_WhenInstanceIdMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            var useCase = new GetGameObjectInfoUseCase(dispatcher, ops);
            var handler = new GameObjectInfoHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.GameObjectInfo);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
        }
    }
}
