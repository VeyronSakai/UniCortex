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
    internal sealed class SendInputSystemKeyEventHandlerTest
    {
        [Test]
        public void Handle_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputSystemSimulationOperations();
            var useCase = new SendInputSystemKeyEventUseCase(dispatcher, ops);
            var handler = new SendInputSystemKeyEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputSystemKey,
                "{\"key\":\"Space\",\"eventType\":\"press\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual("Space", ops.LastKey);
            Assert.AreEqual("press", ops.LastKeyEventType);
        }

        [Test]
        public void Handle_Returns200_WithDefaultEventType()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputSystemSimulationOperations();
            var useCase = new SendInputSystemKeyEventUseCase(dispatcher, ops);
            var handler = new SendInputSystemKeyEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputSystemKey,
                "{\"key\":\"A\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual("press", ops.LastKeyEventType);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputSystemSimulationOperations();
            var useCase = new SendInputSystemKeyEventUseCase(dispatcher, ops);
            var handler = new SendInputSystemKeyEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputSystemKey, "");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }

        [Test]
        public void Handle_Returns400_WhenKeyMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputSystemSimulationOperations();
            var useCase = new SendInputSystemKeyEventUseCase(dispatcher, ops);
            var handler = new SendInputSystemKeyEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputSystemKey,
                "{\"eventType\":\"press\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("key is required", context.ResponseBody);
        }
    }
}
