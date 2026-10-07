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
    internal sealed class DragMouseHandlerTest
    {
        private static (RequestRouter router, SpyInputOperations ops, SpyUIPointerTargetOperations uiPointerTargetOps)
            CreateRouter()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var uiPointerTargetOps = new SpyUIPointerTargetOperations();
            var resolver = new PointerPositionResolver(dispatcher, uiPointerTargetOps);
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var useCase = new DragMouseUseCase(new PlayerLoopRunner(dispatcher, playerLoopDispatcher), resolver,
                ops, playerLoopDispatcher.Time);
            var handler = new DragMouseHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);
            return (router, ops, uiPointerTargetOps);
        }

        private static FakeRequestContext CreateContext(string body)
        {
            return new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouseDrag, body);
        }

        [Test]
        public void Handle_Returns200_AndDragsOverGivenDuration()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = CreateContext(
                "{\"fromX\":0.0,\"fromY\":0.0,\"toX\":100.0,\"toY\":40.0,\"button\":\"right\",\"duration\":0.5}");

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
            StringAssert.Contains("\"fromX\":0.0", context.ResponseBody);
            StringAssert.Contains("\"fromY\":0.0", context.ResponseBody);
            StringAssert.Contains("\"toX\":100.0", context.ResponseBody);
            StringAssert.Contains("\"toY\":40.0", context.ResponseBody);
        }

        [Test]
        public void Handle_UsesDefaults_WhenOptionalFieldsAreOmitted()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = CreateContext("{\"fromX\":0.0,\"fromY\":0.0,\"toX\":100.0,\"toY\":0.0}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            // The default duration is shorter than a frame of FakePlayerLoopDispatcher (0.25s), so it presses, moves
            // to the end once in the next frame, and releases.
            Assert.Less(DragMouseUseCase.DefaultDuration, 0.25f);
            Assert.AreEqual(3, ops.MouseEventHistory.Count);
            Assert.AreEqual(MouseAction.Press, ops.MouseEventHistory[0].Action);
            Assert.AreEqual(MouseButton.Left, ops.MouseEventHistory[0].Button);
            Assert.AreEqual(MouseAction.Move, ops.MouseEventHistory[1].Action);
            Assert.AreEqual(100f, ops.MouseEventHistory[1].X);
            Assert.AreEqual(MouseAction.Release, ops.MouseEventHistory[2].Action);
        }

        [Test]
        public void Handle_Returns200_WithTargets()
        {
            // Arrange
            var (router, ops, uiPointerTargetOps) = CreateRouter();
            uiPointerTargetOps.TargetCentersToReturn[111] = (10f, 20f);
            uiPointerTargetOps.TargetCentersToReturn[222] = (30f, 40f);
            var context = CreateContext("{\"fromInstanceId\":111,\"toInstanceId\":222,\"duration\":0.5}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(2, uiPointerTargetOps.GetTargetCenterCallCount);
            Assert.AreEqual(10f, ops.MouseEventHistory[0].X);
            Assert.AreEqual(20f, ops.MouseEventHistory[0].Y);
            Assert.AreEqual(30f, ops.MouseEventHistory[^1].X);
            Assert.AreEqual(40f, ops.MouseEventHistory[^1].Y);
        }

        [TestCase("{\"fromX\":0.0,\"fromY\":0.0}", "Specify either toX and toY, or toInstanceId.")]
        [TestCase("{\"fromX\":0.0,\"fromY\":0.0,\"toX\":1.0}", "toX and toY must be specified together.")]
        [TestCase("{\"fromX\":0.0,\"fromY\":0.0,\"toX\":1.0,\"toY\":1.0,\"toInstanceId\":1}",
            "Specify either toX and toY, or toInstanceId.")]
        [TestCase("{\"fromX\":0.0,\"fromY\":0.0,\"toInstanceId\":0}", "toInstanceId must not be 0.")]
        [TestCase("{\"toX\":1.0,\"toY\":1.0}", "Specify either fromX and fromY, or fromInstanceId.")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"toX\":1.0,\"toY\":1.0}",
            "Specify either fromX and fromY, or fromInstanceId.")]
        [TestCase("{\"fromX\":0.0,\"fromY\":0.0,\"toX\":1.0,\"toY\":1.0,\"duration\":-1.0}",
            "duration must be 0 or greater.")]
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
