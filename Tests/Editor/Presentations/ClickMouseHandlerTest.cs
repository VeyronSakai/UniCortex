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
    internal sealed class ClickMouseHandlerTest
    {
        private static (RequestRouter router, SpyInputOperations ops, SpyPointerTargetOperations pointerTargetOps)
            CreateRouter()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations();
            var resolver = new PointerPositionResolver(dispatcher, pointerTargetOps);
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var useCase = new ClickMouseUseCase(new PlayerLoopRunner(dispatcher, playerLoopDispatcher), resolver,
                ops, playerLoopDispatcher.Time);
            var handler = new ClickMouseHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);
            return (router, ops, pointerTargetOps);
        }

        private static FakeRequestContext CreateContext(string body)
        {
            return new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouseClick, body);
        }

        [Test]
        public void Handle_Returns200_WithCoordinates()
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            var context = CreateContext($"{{\"x\":100.0,\"y\":200.0,\"button\":\"{MouseButton.Right}\"}}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(2, ops.MouseEventHistory.Count);
            Assert.AreEqual(MouseAction.Press, ops.MouseEventHistory[0].Action);
            Assert.AreEqual(100f, ops.MouseEventHistory[0].X);
            Assert.AreEqual(200f, ops.MouseEventHistory[0].Y);
            Assert.AreEqual(MouseButton.Right, ops.MouseEventHistory[0].Button);
            Assert.AreEqual(0, pointerTargetOps.GetTargetCenterCallCount);
            StringAssert.Contains("\"x\":100.0", context.ResponseBody);
            StringAssert.Contains("\"y\":200.0", context.ResponseBody);
        }

        [Test]
        public void Handle_UsesLeftButton_WhenButtonIsOmitted()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = CreateContext("{\"x\":0.0,\"y\":0.0}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(MouseButton.Left, ops.MouseEventHistory[0].Button);
        }

        [Test]
        public void Handle_Returns200_WithInstanceId()
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            pointerTargetOps.TargetCenterToReturn = (320f, 180f);
            var context = CreateContext("{\"instanceId\":12345}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(12345, pointerTargetOps.LastInstanceId);
            Assert.AreEqual(320f, ops.MouseEventHistory[0].X);
            Assert.AreEqual(180f, ops.MouseEventHistory[0].Y);
            StringAssert.Contains("\"x\":320.0", context.ResponseBody);
            StringAssert.Contains("\"y\":180.0", context.ResponseBody);
        }

        [Test]
        public void Handle_PassesHoldDuration_ToUseCase()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var playerLoopDispatcher = new FakePlayerLoopDispatcher();
            var releaseFrame = -1;
            playerLoopDispatcher.OnFrame = frame =>
            {
                if (releaseFrame < 0 && ops.MouseEventHistory.Exists(e => e.Action == MouseAction.Release))
                {
                    releaseFrame = frame - 1;
                }
            };
            var resolver = new PointerPositionResolver(dispatcher, new SpyPointerTargetOperations());
            var handler = new ClickMouseHandler(new ClickMouseUseCase(
                new PlayerLoopRunner(dispatcher, playerLoopDispatcher), resolver, ops, playerLoopDispatcher.Time));
            var router = new RequestRouter();
            handler.Register(router);
            var context = CreateContext("{\"x\":0.0,\"y\":0.0,\"holdDuration\":0.5}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            // Frames are 0.25 seconds apart, so the release is in frame 2 (0.5s after the press in frame 0).
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(2, releaseFrame);
        }

        [Test]
        public void Handle_Returns400_WhenHoldDurationIsNegative()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = CreateContext("{\"x\":0.0,\"y\":0.0,\"holdDuration\":-1.0}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("holdDuration must be 0 or greater.", context.ResponseBody);
            CollectionAssert.IsEmpty(ops.MouseEventHistory);
        }

        [TestCase("", "Specify either x and y, or instanceId.")]
        [TestCase("{\"button\":\"left\"}", "Specify either x and y, or instanceId.")]
        [TestCase("{\"x\":1.0,\"y\":2.0,\"instanceId\":12345}", "Specify either x and y, or instanceId.")]
        [TestCase("{\"x\":1.0}", "x and y must be specified together.")]
        [TestCase("{\"instanceId\":0}", "instanceId must not be 0.")]
        public void Handle_Returns400_WhenPositionIsInvalid(string body, string expectedMessage)
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            var context = CreateContext(body);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains(expectedMessage, context.ResponseBody);
            CollectionAssert.IsEmpty(ops.MouseEventHistory);
            Assert.AreEqual(0, pointerTargetOps.GetTargetCenterCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenTargetNotFound()
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            pointerTargetOps.ExceptionToThrow = new ArgumentException("GameObject with instanceId 999 not found.");
            var context = CreateContext("{\"instanceId\":999}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("not found", context.ResponseBody);
            CollectionAssert.IsEmpty(ops.MouseEventHistory);
        }
    }
}
