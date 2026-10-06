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
    internal sealed class DragPointerHandlerTest
    {
        private static (RequestRouter router, SpyInputOperations ops, SpyPointerTargetOperations pointerTargetOps)
            CreateRouter()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations();
            var resolver = new PointerPositionResolver(dispatcher, pointerTargetOps);
            var useCase = new DragPointerUseCase(dispatcher, new FakePlayerLoopDispatcher(), resolver, ops);
            var handler = new DragPointerHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);
            return (router, ops, pointerTargetOps);
        }

        private static FakeRequestContext CreateContext(string body)
        {
            return new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputPointerDrag, body);
        }

        [Test]
        public void Handle_Returns200_AndDragsOverGivenFrames()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = CreateContext(
                "{\"x\":0.0,\"y\":0.0,\"toX\":100.0,\"toY\":40.0,\"button\":\"right\",\"frames\":2}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(4, ops.MouseEventHistory.Count);
            Assert.AreEqual(MouseAction.Press, ops.MouseEventHistory[0].Action);
            Assert.AreEqual(MouseButton.Right, ops.MouseEventHistory[0].Button);
            Assert.AreEqual(MouseAction.Move, ops.MouseEventHistory[1].Action);
            Assert.AreEqual(50f, ops.MouseEventHistory[1].X);
            Assert.AreEqual(20f, ops.MouseEventHistory[1].Y);
            Assert.AreEqual(MouseAction.Move, ops.MouseEventHistory[2].Action);
            Assert.AreEqual(MouseAction.Release, ops.MouseEventHistory[3].Action);
            StringAssert.Contains("\"toX\":100.0", context.ResponseBody);
            StringAssert.Contains("\"toY\":40.0", context.ResponseBody);
        }

        [Test]
        public void Handle_UsesDefaults_WhenOptionalFieldsAreOmitted()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = CreateContext("{\"x\":0.0,\"y\":0.0,\"toX\":100.0,\"toY\":0.0}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            // Press, 10 moves, and release, with no hold frames.
            Assert.AreEqual(DragPointerUseCase.DefaultFrames + 2, ops.MouseEventHistory.Count);
            Assert.AreEqual(MouseButton.Left, ops.MouseEventHistory[0].Button);
        }

        [Test]
        public void Handle_Returns200_WithTargets()
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            pointerTargetOps.TargetCentersToReturn[111] = (10f, 20f);
            pointerTargetOps.TargetCentersToReturn[222] = (30f, 40f);
            var context = CreateContext("{\"instanceId\":111,\"toInstanceId\":222,\"frames\":1}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(2, pointerTargetOps.GetTargetCenterCallCount);
            Assert.AreEqual(10f, ops.MouseEventHistory[0].X);
            Assert.AreEqual(20f, ops.MouseEventHistory[0].Y);
            Assert.AreEqual(30f, ops.MouseEventHistory[^1].X);
            Assert.AreEqual(40f, ops.MouseEventHistory[^1].Y);
        }

        [TestCase("{\"x\":0.0,\"y\":0.0}", "Specify either toX and toY, or toInstanceId.")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"toX\":1.0}", "toX and toY must be specified together.")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"toX\":1.0,\"toY\":1.0,\"toInstanceId\":1}",
            "Specify either toX and toY, or toInstanceId.")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"toInstanceId\":0}", "toInstanceId must not be 0.")]
        [TestCase("{\"toX\":1.0,\"toY\":1.0}", "Specify either x and y, or instanceId.")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"toX\":1.0,\"toY\":1.0,\"frames\":0}", "frames must be 1 or greater.")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"toX\":1.0,\"toY\":1.0,\"holdFrames\":-1}",
            "holdFrames must be 0 or greater.")]
        public void Handle_Returns400_WhenParametersAreInvalid(string body, string expectedMessage)
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = CreateContext(body);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains(expectedMessage, context.ResponseBody);
            CollectionAssert.IsEmpty(ops.MouseEventHistory);
        }
    }
}
