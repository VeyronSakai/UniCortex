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
        [Test]
        public void Handle_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, ops);
            var handler = new SendMouseEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                $"{{\"x\":100.0,\"y\":200.0,\"button\":\"{MouseButton.Left}\",\"eventType\":\"{InputEventType.Press}\"}}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

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
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, ops);
            var handler = new SendMouseEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                "{\"x\":50.0,\"y\":75.0}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(MouseButton.Left, ops.LastMouseButton);
            // Default eventType is "click", which decomposes into press then release.
            Assert.AreEqual(2, ops.SendMouseEventCallCount);
            Assert.AreEqual(InputEventType.Press, ops.MouseEventHistory[0].EventType);
            Assert.AreEqual(InputEventType.Release, ops.MouseEventHistory[1].EventType);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, ops);
            var handler = new SendMouseEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse, "");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }
    }
}
