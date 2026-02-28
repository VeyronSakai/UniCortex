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
    internal sealed class CreateGameObjectHandlerTest
    {
        [Test]
        public void HandleCreate_Returns200_WhenNameProvided()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            ops.CreateResult = new CreateGameObjectResponse("MyCube", 999);
            var useCase = new CreateGameObjectUseCase(dispatcher, ops);
            var handler = new CreateGameObjectHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.GameObjectCreate, "{\"name\":\"MyCube\",\"primitive\":\"Cube\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("MyCube", context.ResponseBody);
        }

        [Test]
        public void HandleCreate_Returns400_WhenNameMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            var useCase = new CreateGameObjectUseCase(dispatcher, ops);
            var handler = new CreateGameObjectHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.GameObjectCreate, "{}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
            StringAssert.Contains("name is required", context.ResponseBody);
        }
    }
}
