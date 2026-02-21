using System.Threading;
using EditorBridge.Editor.Domains.Models;
using EditorBridge.Editor.Infrastructures;
using EditorBridge.Editor.Presentations;
using EditorBridge.Editor.Tests.TestDoubles;
using EditorBridge.Editor.UseCases;
using NUnit.Framework;

namespace EditorBridge.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class PingHandlerTest
    {
        [Test]
        public void HandlePing_Returns200WithPongResponse()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new PingUseCase(dispatcher);
            var handler = new PingHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext
            {
                HttpMethod = "GET",
                Path = ApiRoutes.Ping
            };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("pong", context.ResponseBody);
        }
    }
}
