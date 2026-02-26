using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.Console;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class ConsoleClearHandlerTest
    {
        [Test]
        public void HandleConsoleClear_Returns200WithSuccess()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var collector = new SpyConsoleLogCollector();
            var useCase = new ClearConsoleLogsUseCase(dispatcher, collector);
            var handler = new ConsoleClearHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext { HttpMethod = "POST", Path = ApiRoutes.ConsoleClear };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("\"success\":true", context.ResponseBody);
        }

        [Test]
        public void HandleConsoleClear_CallsClearOnCollector()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var collector = new SpyConsoleLogCollector();
            var useCase = new ClearConsoleLogsUseCase(dispatcher, collector);
            var handler = new ConsoleClearHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext { HttpMethod = "POST", Path = ApiRoutes.ConsoleClear };

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, collector.ClearCallCount);
        }
    }
}
