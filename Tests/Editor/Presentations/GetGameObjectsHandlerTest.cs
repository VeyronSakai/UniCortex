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
    internal sealed class GetGameObjectsHandlerTest
    {
        [Test]
        public void HandleGet_Returns200_WithResults()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            ops.GetResult = new List<GameObjectSearchResult>
            {
                new GameObjectSearchResult("Player", 100, true, "Untagged", 0, false, false,
                    new List<string> { "Transform" })
            };
            var useCase = new GetGameObjectsUseCase(dispatcher, ops);
            var handler = new GetGameObjectsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.GameObjects);
            context.SetQueryParameter("query", "Player");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("Player", context.ResponseBody);
            Assert.AreEqual("Player", ops.LastGetQuery);
        }
    }
}
