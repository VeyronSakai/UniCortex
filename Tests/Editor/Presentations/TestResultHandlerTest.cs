using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Tests;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class TestResultHandlerTest
    {
        [Test]
        public void HandleGetResult_Returns200WithStoredResult()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var store = new FakeTestResultStore { Result = "{\"passed\":1,\"failed\":0,\"skipped\":0}" };
            var handler = new TestResultHandler(dispatcher, store);
            var router = new RequestRouter();
            handler.Register(router);
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TestsResult);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual("{\"passed\":1,\"failed\":0,\"skipped\":0}", context.ResponseBody);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void HandleGetResult_WhileRunIsPending_ReturnsEmptyBody()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var store = new FakeTestResultStore { Result = "{\"passed\":1}" };
            store.MarkPending();
            var handler = new TestResultHandler(dispatcher, store);
            var router = new RequestRouter();
            handler.Register(router);
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.TestsResult);

            // Act
            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(string.Empty, context.ResponseBody);
        }
    }
}
