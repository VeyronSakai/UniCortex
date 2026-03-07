using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UniCortex.Editor.Handlers.Editor;
using UnityEditor;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class PingHandlerTest
    {
        [Test]
        public void HandlePing_Verbose_Returns200WithPongResponse()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new PingUseCase(dispatcher);
            var handler = new PingHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.Ping);
            context.SetQueryParameter(QueryParameterNames.Verbose, "true");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("pong", context.ResponseBody);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void HandlePing_NotVerbose_Returns200WithoutConsoleLog()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new PingUseCase(dispatcher);
            var handler = new PingHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.Ping);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("pong", context.ResponseBody);
            Assert.AreEqual(0, dispatcher.CallCount);
        }
    }
}
