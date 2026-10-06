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
    internal sealed class GameObjectButtonHandlerTest
    {
        // Uses click as a representative of the handlers for click, press and release.
        private static (RequestRouter router, SpyInputOperations ops, SpyPointerTargetOperations pointerTargetOps)
            CreateRouter()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations();
            var resolver = new PointerPositionResolver(dispatcher, pointerTargetOps);
            var useCase = new ClickPointerUseCase(dispatcher, resolver, ops);
            var handler = new GameObjectButtonHandler(ApiRoutes.InputGameObjectClick, useCase.ExecuteAsync);

            var router = new RequestRouter();
            handler.Register(router);
            return (router, ops, pointerTargetOps);
        }

        private static FakeRequestContext CreateContext(string body)
        {
            return new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputGameObjectClick, body);
        }

        [Test]
        public void Handle_Returns200_AndClicksAtTargetCenter()
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
            Assert.AreEqual(2, ops.MouseEventHistory.Count);
            Assert.AreEqual(320f, ops.MouseEventHistory[0].X);
            Assert.AreEqual(180f, ops.MouseEventHistory[0].Y);
            Assert.AreEqual(MouseButton.Left, ops.MouseEventHistory[0].Button);
            StringAssert.Contains("\"x\":320.0", context.ResponseBody);
            StringAssert.Contains("\"y\":180.0", context.ResponseBody);
        }

        [TestCase("")]
        [TestCase("{\"button\":\"left\"}")]
        [TestCase("{\"x\":1.0,\"y\":2.0}")]
        [TestCase("{\"instanceId\":0}")]
        public void Handle_Returns400_WhenInstanceIdIsMissingOrZero(string body)
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            var context = CreateContext(body);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("instanceId is required and must not be 0.", context.ResponseBody);
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
