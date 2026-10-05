using System;
using System.Threading;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using UniCortex.Editor.Domains.Models;
using NUnit.Framework;
using UniCortex.Editor.Handlers.Input;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class SendMouseEventHandlerTest
    {
        private static (RequestRouter router, SpyInputOperations ops, SpyPointerTargetOperations pointerTargetOps)
            CreateRouter()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, new FakePlayerLoopDispatcher(), ops,
                pointerTargetOps);
            var handler = new SendMouseEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);
            return (router, ops, pointerTargetOps);
        }

        [Test]
        public void Handle_Returns200_WhenValid()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                $"{{\"x\":100.0,\"y\":200.0,\"button\":\"{MouseButton.Left}\",\"eventType\":\"{InputEventType.Press}\"}}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(100f, ops.LastMouseX);
            Assert.AreEqual(200f, ops.LastMouseY);
            Assert.AreEqual(MouseButton.Left, ops.LastMouseButton);
            Assert.AreEqual(InputEventType.Press, ops.LastMouseEventType);
        }

        [Test]
        public void Handle_Returns200_WithDefaults()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                "{\"x\":50.0,\"y\":75.0}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(MouseButton.Left, ops.LastMouseButton);
            // Default eventType is "click", which decomposes into press then release.
            Assert.AreEqual(2, ops.SendMouseEventCallCount);
            Assert.AreEqual(InputEventType.Press, ops.MouseEventHistory[0].EventType);
            Assert.AreEqual(InputEventType.Release, ops.MouseEventHistory[1].EventType);
        }

        [Test]
        public void Handle_Returns200_WhenCoordinatesAreZero()
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                "{\"x\":0.0,\"y\":0.0,\"eventType\":\"move\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, ops.SendMouseEventCallCount);
            Assert.AreEqual(0, pointerTargetOps.GetTargetCenterCallCount);
        }

        [Test]
        public void Handle_Returns200_WithInstanceId()
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            pointerTargetOps.TargetCenterToReturn = (320f, 180f);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                "{\"instanceId\":12345}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(12345, pointerTargetOps.LastInstanceId);
            Assert.AreEqual(2, ops.SendMouseEventCallCount);
            Assert.AreEqual(320f, ops.LastMouseX);
            Assert.AreEqual(180f, ops.LastMouseY);
            StringAssert.Contains("\"x\":320.0", context.ResponseBody);
            StringAssert.Contains("\"y\":180.0", context.ResponseBody);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            // Arrange
            var (router, _, _) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse, "");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }

        [Test]
        public void Handle_Returns400_WhenNeitherCoordinatesNorTarget()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                "{\"button\":\"left\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, ops.SendMouseEventCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenBothCoordinatesAndTarget()
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                "{\"x\":1.0,\"y\":2.0,\"instanceId\":12345}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, ops.SendMouseEventCallCount);
            Assert.AreEqual(0, pointerTargetOps.GetTargetCenterCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenInstanceIdIsZero()
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                "{\"instanceId\":0}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, ops.SendMouseEventCallCount);
            Assert.AreEqual(0, pointerTargetOps.GetTargetCenterCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenOnlyXIsGiven()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse, "{\"x\":1.0}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, ops.SendMouseEventCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenTargetNotFound()
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            pointerTargetOps.ExceptionToThrow = new ArgumentException("GameObject with instanceId 999 not found.");
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                "{\"instanceId\":999}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("not found", context.ResponseBody);
            Assert.AreEqual(0, ops.SendMouseEventCallCount);
        }

        [Test]
        public void Handle_Drag_Returns200_AndDragsOverGivenFrames()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                "{\"x\":0.0,\"y\":0.0,\"toX\":100.0,\"toY\":40.0,\"eventType\":\"drag\",\"frames\":2}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(4, ops.SendMouseEventCallCount);
            Assert.AreEqual(InputEventType.Press, ops.MouseEventHistory[0].EventType);
            Assert.AreEqual(InputEventType.Move, ops.MouseEventHistory[1].EventType);
            Assert.AreEqual(50f, ops.MouseEventHistory[1].X);
            Assert.AreEqual(20f, ops.MouseEventHistory[1].Y);
            Assert.AreEqual(InputEventType.Move, ops.MouseEventHistory[2].EventType);
            Assert.AreEqual(InputEventType.Release, ops.MouseEventHistory[3].EventType);
            StringAssert.Contains("\"toX\":100.0", context.ResponseBody);
            StringAssert.Contains("\"toY\":40.0", context.ResponseBody);
        }

        [Test]
        public void Handle_Drag_UsesDefaultFrames_WhenFramesIsOmitted()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                "{\"x\":0.0,\"y\":0.0,\"toX\":100.0,\"toY\":0.0,\"eventType\":\"drag\"}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            // Press, 10 moves, and release.
            Assert.AreEqual(12, ops.SendMouseEventCallCount);
        }

        [Test]
        public void Handle_Drag_Returns200_WithTargets()
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            pointerTargetOps.TargetCentersToReturn[111] = (10f, 20f);
            pointerTargetOps.TargetCentersToReturn[222] = (30f, 40f);
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                "{\"instanceId\":111,\"toInstanceId\":222,\"eventType\":\"drag\",\"frames\":1}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(2, pointerTargetOps.GetTargetCenterCallCount);
            Assert.AreEqual(10f, ops.MouseEventHistory[0].X);
            Assert.AreEqual(20f, ops.MouseEventHistory[0].Y);
            Assert.AreEqual(30f, ops.LastMouseX);
            Assert.AreEqual(40f, ops.LastMouseY);
        }

        [TestCase("{\"x\":0.0,\"y\":0.0,\"eventType\":\"drag\"}", "Specify either toX and toY, or toInstanceId.")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"toX\":1.0,\"eventType\":\"drag\"}", "toX and toY must be specified together.")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"toX\":1.0,\"toY\":1.0,\"toInstanceId\":1,\"eventType\":\"drag\"}",
            "Specify either toX and toY, or toInstanceId.")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"toInstanceId\":0,\"eventType\":\"drag\"}", "toInstanceId must not be 0.")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"toX\":1.0,\"toY\":1.0,\"eventType\":\"drag\",\"frames\":0}",
            "frames must be 1 or greater.")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"toX\":1.0,\"toY\":1.0,\"eventType\":\"drag\",\"holdFrames\":-1}",
            "holdFrames must be 0 or greater.")]
        [TestCase("{\"toX\":1.0,\"toY\":1.0,\"eventType\":\"drag\"}", "Specify either x and y, or instanceId.")]
        public void Handle_Drag_Returns400_WhenParametersAreInvalid(string body, string expectedMessage)
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse, body);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains(expectedMessage, context.ResponseBody);
            Assert.AreEqual(0, ops.SendMouseEventCallCount);
        }

        [TestCase("{\"x\":0.0,\"y\":0.0,\"toX\":1.0,\"toY\":1.0}")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"eventType\":\"press\",\"toInstanceId\":1}")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"eventType\":\"move\",\"frames\":5}")]
        [TestCase("{\"x\":0.0,\"y\":0.0,\"eventType\":\"click\",\"holdFrames\":5}")]
        public void Handle_Returns400_WhenDragParametersAreGivenWithoutDrag(string body)
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse, body);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("only valid with eventType", context.ResponseBody);
            Assert.AreEqual(0, ops.SendMouseEventCallCount);
        }
    }
}
