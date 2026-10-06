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
    internal sealed class MovePointerHandlerTest
    {
        private static (RequestRouter router, SpyInputOperations ops) CreateRouter()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var resolver = new PointerPositionResolver(dispatcher, new SpyPointerTargetOperations());
            var handler = new MovePointerHandler(new MovePointerUseCase(dispatcher, resolver, ops));

            var router = new RequestRouter();
            handler.Register(router);
            return (router, ops);
        }

        [Test]
        public void Handle_Returns200_AndMoves()
        {
            // Arrange
            var (router, ops) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputPointerMove,
                "{\"x\":0.0,\"y\":0.0}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, ops.MouseEventHistory.Count);
            Assert.AreEqual(MouseAction.Move, ops.MouseEventHistory[0].Action);
            Assert.AreEqual(0f, ops.MouseEventHistory[0].X);
            Assert.AreEqual(0f, ops.MouseEventHistory[0].Y);
        }

        [Test]
        public void Handle_Returns400_WhenPositionIsMissing()
        {
            // Arrange
            var (router, ops) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputPointerMove, "{}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Specify either x and y, or instanceId.", context.ResponseBody);
            CollectionAssert.IsEmpty(ops.MouseEventHistory);
        }
    }
}
