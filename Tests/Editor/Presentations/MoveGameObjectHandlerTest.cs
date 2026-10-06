using System;
using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Input;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using MouseAction = UniCortex.Editor.Tests.TestDoubles.SpyInputOperations.MouseAction;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class MoveGameObjectHandlerTest
    {
        private static (RequestRouter router, SpyInputOperations ops, SpyPointerTargetOperations pointerTargetOps)
            CreateRouter()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations();
            var resolver = new PointerPositionResolver(dispatcher, pointerTargetOps);
            var handler = new MoveGameObjectHandler(new MovePointerUseCase(dispatcher, resolver, ops));

            var router = new RequestRouter();
            handler.Register(router);
            return (router, ops, pointerTargetOps);
        }

        [Test]
        public void Handle_Returns200_AndMovesToTargetCenter()
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            pointerTargetOps.TargetCenterToReturn = (40f, 60f);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputGameObjectMove,
                "{\"instanceId\":12345}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(12345, pointerTargetOps.LastInstanceId);
            Assert.AreEqual(MouseAction.Move, ops.MouseEventHistory[0].Action);
            Assert.AreEqual(40f, ops.MouseEventHistory[0].X);
            Assert.AreEqual(60f, ops.MouseEventHistory[0].Y);
        }

        [Test]
        public void Handle_Returns400_WhenInstanceIdIsMissing()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputGameObjectMove, "{}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("instanceId is required and must not be 0.", context.ResponseBody);
            CollectionAssert.IsEmpty(ops.MouseEventHistory);
        }
    }
}
