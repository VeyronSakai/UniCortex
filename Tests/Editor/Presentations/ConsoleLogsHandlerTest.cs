using System.Collections.Generic;
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
    internal sealed class ConsoleLogsHandlerTest
    {
        [Test]
        public void HandleConsoleLogs_Returns200WithLogs()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var collector =
                new SpyConsoleLogCollector(new List<ConsoleLogEntry> { new("test message", "", "Log", ""), });
            var useCase = new GetConsoleLogsUseCase(dispatcher, collector);
            var handler = new ConsoleLogsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.ConsoleLogs);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("test message", context.ResponseBody);
        }

        [Test]
        public void HandleConsoleLogs_UsesCountQueryParameter()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var collector = new SpyConsoleLogCollector(new List<ConsoleLogEntry>());
            var useCase = new GetConsoleLogsUseCase(dispatcher, collector);
            var handler = new ConsoleLogsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.ConsoleLogs);
            context.SetQueryParameter("count", "50");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            Assert.AreEqual(50, collector.LastCount);
        }

        [Test]
        public void HandleConsoleLogs_DefaultsToCount100()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var collector = new SpyConsoleLogCollector(new List<ConsoleLogEntry>());
            var useCase = new GetConsoleLogsUseCase(dispatcher, collector);
            var handler = new ConsoleLogsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.ConsoleLogs);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            Assert.AreEqual(100, collector.LastCount);
        }

        [Test]
        public void HandleConsoleLogs_PassesStackTraceParameter()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var collector = new SpyConsoleLogCollector(new List<ConsoleLogEntry>());
            var useCase = new GetConsoleLogsUseCase(dispatcher, collector);
            var handler = new ConsoleLogsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.ConsoleLogs);
            context.SetQueryParameter("stackTrace", "true");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            Assert.IsTrue(collector.LastIncludeStackTrace);
        }

        [Test]
        public void HandleConsoleLogs_PassesTypeFilterParameters()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var collector = new SpyConsoleLogCollector(new List<ConsoleLogEntry>());
            var useCase = new GetConsoleLogsUseCase(dispatcher, collector);
            var handler = new ConsoleLogsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("GET", ApiRoutes.ConsoleLogs);
            context.SetQueryParameter("log", "false");
            context.SetQueryParameter("warning", "false");
            context.SetQueryParameter("error", "true");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            Assert.IsFalse(collector.LastShowLog);
            Assert.IsFalse(collector.LastShowWarning);
            Assert.IsTrue(collector.LastShowError);
        }
    }
}
