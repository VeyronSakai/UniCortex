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

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameObjectCreate, "{\"name\":\"MyCube\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
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

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameObjectCreate, "{}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("name is required", context.ResponseBody);
        }

        [Test]
        public void HandleCreate_PassesParentSiblingIndexAndRectTransformOptions()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            var useCase = new CreateGameObjectUseCase(dispatcher, ops);
            var handler = new CreateGameObjectHandler(useCase);
            var router = new RequestRouter();
            handler.Register(router);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameObjectCreate,
                "{\"name\":\"Panel\",\"parentInstanceId\":42,\"siblingIndex\":0,\"useRectTransform\":true}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(42, ops.LastCreateParentInstanceId);
            Assert.AreEqual(0, ops.LastCreateSiblingIndex);
            Assert.IsTrue(ops.LastCreateUseRectTransform);
        }

        [Test]
        public void HandleCreate_PassesNullSiblingIndex_WhenOmitted()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            var useCase = new CreateGameObjectUseCase(dispatcher, ops);
            var handler = new CreateGameObjectHandler(useCase);
            var router = new RequestRouter();
            handler.Register(router);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameObjectCreate,
                "{\"name\":\"Obj\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(0, ops.LastCreateParentInstanceId);
            Assert.IsNull(ops.LastCreateSiblingIndex);
            Assert.IsFalse(ops.LastCreateUseRectTransform);
        }

        [Test]
        public void HandleCreate_Returns400_WhenSiblingIndexNegative()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            var useCase = new CreateGameObjectUseCase(dispatcher, ops);
            var handler = new CreateGameObjectHandler(useCase);
            var router = new RequestRouter();
            handler.Register(router);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.GameObjectCreate,
                "{\"name\":\"Obj\",\"siblingIndex\":-1}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, ops.CreateCallCount);
        }
    }
}
