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
    internal sealed class FindGameObjectsHandlerTest
    {
        [Test]
        public void HandleFind_Returns200_WithResults()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            ops.FindResult = new List<GameObjectBasicInfo>
            {
                new GameObjectBasicInfo("Player", 100, true)
            };
            var useCase = new FindGameObjectsUseCase(dispatcher, ops);
            var handler = new FindGameObjectsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext { HttpMethod = "GET", Path = ApiRoutes.GameObjectFind };
            context.SetQueryParameter("name", "Player");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("Player", context.ResponseBody);
            Assert.AreEqual("Player", ops.LastFindName);
        }
    }
}
