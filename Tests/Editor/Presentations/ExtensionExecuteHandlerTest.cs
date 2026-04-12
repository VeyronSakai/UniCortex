using System;
using System.Text.RegularExpressions;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Extension;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class ExtensionExecuteHandlerTest
    {
        private ExtensionRegistry _registry;
        private FakeMainThreadDispatcher _dispatcher;
        private RequestRouter _router;
        private StubExtensionHandler _stubHandler;

        [SetUp]
        public void SetUp()
        {
            _registry = new ExtensionRegistry(discover: false);
            _stubHandler = new StubExtensionHandler("test_extension", "Test extension");
            _stubHandler.ExecuteResult = "test result";
            _registry.RegisterForTest(_stubHandler);
            _dispatcher = new FakeMainThreadDispatcher();
            var handler = new ExtensionExecuteHandler(_registry, _dispatcher);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void HandleExecute_Returns200_WithResult()
        {
            var body = "{\"name\":\"test_extension\",\"arguments\":\"{\\\"key\\\":\\\"value\\\"}\"}";
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ExtensionExecute, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("test result", context.ResponseBody);
            Assert.AreEqual(1, _dispatcher.CallCount);
        }

        [Test]
        public void HandleExecute_PassesArguments_ToHandler()
        {
            var body = "{\"name\":\"test_extension\",\"arguments\":\"{\\\"x\\\":1}\"}";
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ExtensionExecute, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("{\"x\":1}", _stubHandler.LastArgumentsJson);
        }

        [Test]
        public void HandleExecute_Returns400_WhenNameMissing()
        {
            var body = "{\"arguments\":\"{}\"}";
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ExtensionExecute, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
        }

        [Test]
        public void HandleExecute_Returns404_WhenExtensionNotFound()
        {
            var body = "{\"name\":\"nonexistent\"}";
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ExtensionExecute, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.NotFound, context.ResponseStatusCode);
        }

        [Test]
        public void HandleExecute_Returns500_WhenHandlerThrows()
        {
            _stubHandler.ExecuteException = new InvalidOperationException("Boom");
            var body = "{\"name\":\"test_extension\"}";
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ExtensionExecute, body);

            LogAssert.Expect(LogType.Error, new Regex("Boom"));
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.InternalServerError, context.ResponseStatusCode);
        }

        [Test]
        public void HandleExecute_UsesEmptyString_WhenArgumentsNull()
        {
            var body = "{\"name\":\"test_extension\"}";
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.ExtensionExecute, body);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("", _stubHandler.LastArgumentsJson);
        }
    }
}
