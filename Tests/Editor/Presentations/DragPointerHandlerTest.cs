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
    internal sealed class DragPointerHandlerTest
    {
        private static (RequestRouter router, SpyInputOperations ops) CreateRouter()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var resolver = new PointerPositionResolver(dispatcher, new SpyPointerTargetOperations());
            var useCase = new DragPointerUseCase(dispatcher, new FakePlayerLoopDispatcher(), resolver, ops);
            var handler = new DragPointerHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);
            return (router, ops);
        }

        private static FakeRequestContext CreateContext(string body)
        {
            return new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputPointerDrag, body);
        }

        [Test]
        public void Handle_Returns200_AndDragsOverGivenFrames()
        {
            // Arrange
            var (router, ops) = CreateRouter();
            var context = CreateContext(
                "{\"fromX\":0.0,\"fromY\":0.0,\"toX\":100.0,\"toY\":40.0,\"button\":\"right\",\"frames\":2}");

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
            var (router, ops) = CreateRouter();
            var context = CreateContext("{\"fromX\":0.0,\"fromY\":0.0,\"toX\":100.0,\"toY\":0.0}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            // Press, the default number of moves, and release, with no hold frames.
            Assert.AreEqual(DragPointerUseCase.DefaultFrames + 2, ops.MouseEventHistory.Count);
            Assert.AreEqual(MouseButton.Left, ops.MouseEventHistory[0].Button);
        }

        [TestCase("", "fromX is required.")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"toX\":1.0,\"toY\":1.0}", "fromX is required.")]
        [TestCase("{\"fromX\":0.0,\"toX\":1.0,\"toY\":1.0}", "fromY is required.")]
        [TestCase("{\"fromX\":0.0,\"fromY\":0.0,\"toY\":1.0}", "toX is required.")]
        [TestCase("{\"fromX\":0.0,\"fromY\":0.0,\"toX\":1.0}", "toY is required.")]
        [TestCase("{\"fromX\":0.0,\"fromY\":0.0,\"toX\":1.0,\"toY\":1.0,\"frames\":0}", "frames must be 1 or greater.")]
        [TestCase("{\"fromX\":0.0,\"fromY\":0.0,\"toX\":1.0,\"toY\":1.0,\"holdFrames\":-1}",
            "holdFrames must be 0 or greater.")]
        public void Handle_Returns400_WhenParametersAreInvalid(string body, string expectedMessage)
        {
            // Arrange
            var (router, ops) = CreateRouter();
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
