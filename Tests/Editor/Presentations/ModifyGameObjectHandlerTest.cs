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
    internal sealed class ModifyGameObjectHandlerTest
    {
        [Test]
        public void HandleModify_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            var useCase = new ModifyGameObjectUseCase(dispatcher, ops);
            var handler = new ModifyGameObjectHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.GameObjectModify, "{\"instanceId\":123,\"name\":\"NewName\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            Assert.AreEqual(123, ops.LastModifyInstanceId);
            Assert.AreEqual("NewName", ops.LastModifyName);
            Assert.IsNull(ops.LastModifyActiveSelf);
            Assert.IsNull(ops.LastModifyTag);
        }

        [Test]
        public void HandleModify_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            var useCase = new ModifyGameObjectUseCase(dispatcher, ops);
            var handler = new ModifyGameObjectHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.GameObjectModify);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(400, context.ResponseStatusCode);
        }
    }
}
