using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
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
            var ops = new SpyInputSimulationOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, ops);
            var handler = new SendMouseEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                "{\"x\":100.0,\"y\":200.0,\"button\":0,\"eventType\":\"mouseDown\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(100f, ops.LastMouseX);
            Assert.AreEqual(200f, ops.LastMouseY);
            Assert.AreEqual(0, ops.LastMouseButton);
            Assert.AreEqual("mouseDown", ops.LastMouseEventType);
        }

        [Test]
        public void Handle_Returns200_WithDefaults()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputSimulationOperations();
            var useCase = new SendMouseEventUseCase(dispatcher, ops);
            var handler = new SendMouseEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputMouse,
                "{\"x\":50.0,\"y\":75.0}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(0, ops.LastMouseButton);
            Assert.AreEqual("mouseDown", ops.LastMouseEventType);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputSimulationOperations();
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
