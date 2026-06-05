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
    internal sealed class DuplicateGameObjectHandlerTest
    {
        [Test]
        public void HandleDuplicate_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            var useCase = new DuplicateGameObjectUseCase(dispatcher, ops);
            var handler = new DuplicateGameObjectHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameObjectDuplicate,
                "{\"instanceId\":123,\"name\":\"Copy\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, ops.DuplicateCallCount);
            Assert.AreEqual(123, ops.LastDuplicateInstanceId);
            Assert.AreEqual("Copy", ops.LastDuplicateName);
        }

        [Test]
        public void HandleDuplicate_PassesNullName_WhenOmitted()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            var useCase = new DuplicateGameObjectUseCase(dispatcher, ops);
            var handler = new DuplicateGameObjectHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameObjectDuplicate,
                "{\"instanceId\":123}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(123, ops.LastDuplicateInstanceId);
            Assert.IsNull(ops.LastDuplicateName);
        }

        [Test]
        public void HandleDuplicate_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            var useCase = new DuplicateGameObjectUseCase(dispatcher, ops);
            var handler = new DuplicateGameObjectHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameObjectDuplicate);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, ops.DuplicateCallCount);
        }

        [Test]
        public void HandleDuplicate_Returns400_WhenInstanceIdMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            var useCase = new DuplicateGameObjectUseCase(dispatcher, ops);
            var handler = new DuplicateGameObjectHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameObjectDuplicate,
                "{\"name\":\"Copy\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, ops.DuplicateCallCount);
        }
    }
}
