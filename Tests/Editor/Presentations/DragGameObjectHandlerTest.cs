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
    internal sealed class DragGameObjectHandlerTest
    {
        private static (RequestRouter router, SpyInputOperations ops, SpyPointerTargetOperations pointerTargetOps)
            CreateRouter()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var pointerTargetOps = new SpyPointerTargetOperations();
            var resolver = new PointerPositionResolver(dispatcher, pointerTargetOps);
            var useCase = new DragPointerUseCase(dispatcher, new FakePlayerLoopDispatcher(), resolver, ops);
            var handler = new DragGameObjectHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);
            return (router, ops, pointerTargetOps);
        }

        private static FakeRequestContext CreateContext(string body)
        {
            return new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputGameObjectDrag, body);
        }

        [Test]
        public void Handle_Returns200_AndDragsBetweenTargetCenters()
        {
            // Arrange
            var (router, ops, pointerTargetOps) = CreateRouter();
            pointerTargetOps.TargetCentersToReturn[111] = (10f, 20f);
            pointerTargetOps.TargetCentersToReturn[222] = (30f, 40f);
            var context = CreateContext("{\"fromInstanceId\":111,\"toInstanceId\":222,\"frames\":1}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(2, pointerTargetOps.GetTargetCenterCallCount);
            Assert.AreEqual(3, ops.MouseEventHistory.Count);
            Assert.AreEqual(MouseAction.Press, ops.MouseEventHistory[0].Action);
            Assert.AreEqual(10f, ops.MouseEventHistory[0].X);
            Assert.AreEqual(20f, ops.MouseEventHistory[0].Y);
            Assert.AreEqual(MouseAction.Release, ops.MouseEventHistory[2].Action);
            Assert.AreEqual(30f, ops.MouseEventHistory[2].X);
            Assert.AreEqual(40f, ops.MouseEventHistory[2].Y);
            StringAssert.Contains("\"fromX\":10.0", context.ResponseBody);
            StringAssert.Contains("\"toX\":30.0", context.ResponseBody);
        }

        [Test]
        public void Handle_UsesDefaultFrames_WhenFramesIsOmitted()
        {
            // Arrange
            var (router, ops, _) = CreateRouter();
            var context = CreateContext("{\"fromInstanceId\":111,\"toInstanceId\":222}");

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(DragPointerUseCase.DefaultFrames + 2, ops.MouseEventHistory.Count);
        }

        [TestCase("", "fromInstanceId is required and must not be 0.")]
        [TestCase("{\"toInstanceId\":222}", "fromInstanceId is required and must not be 0.")]
        [TestCase("{\"fromInstanceId\":111}", "toInstanceId is required and must not be 0.")]
        [TestCase("{\"fromInstanceId\":111,\"toInstanceId\":0}", "toInstanceId is required and must not be 0.")]
        [TestCase("{\"fromInstanceId\":111,\"toInstanceId\":222,\"frames\":0}", "frames must be 1 or greater.")]
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
