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
    internal sealed class SendKeyEventHandlerTest
    {
        [Test]
        public void Handle_Returns200_WhenValid()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputSimulationOperations();
            var useCase = new SendKeyEventUseCase(dispatcher, ops);
            var handler = new SendKeyEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputKey,
                "{\"keyName\":\"space\",\"eventType\":\"keyDown\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual("space", ops.LastKeyName);
            Assert.AreEqual("keyDown", ops.LastKeyEventType);
        }

        [Test]
        public void Handle_Returns200_WithDefaultEventType()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputSimulationOperations();
            var useCase = new SendKeyEventUseCase(dispatcher, ops);
            var handler = new SendKeyEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputKey,
                "{\"keyName\":\"space\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual("keyDown", ops.LastKeyEventType);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputSimulationOperations();
            var useCase = new SendKeyEventUseCase(dispatcher, ops);
            var handler = new SendKeyEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputKey, "");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }

        [Test]
        public void Handle_Returns400_WhenKeyNameMissing()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyInputSimulationOperations();
            var useCase = new SendKeyEventUseCase(dispatcher, ops);
            var handler = new SendKeyEventHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.InputKey,
                "{\"eventType\":\"keyDown\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("keyName is required", context.ResponseBody);
        }
    }
}
