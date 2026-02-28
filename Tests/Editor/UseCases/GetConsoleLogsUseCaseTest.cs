using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetConsoleLogsUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsLogs_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var logs = new List<ConsoleLogEntry>
            {
                new("msg1", "", "Log"), new ConsoleLogEntry("msg2", "", "Warning"),
            };

            var collector = new SpyConsoleLogCollector(logs);
            var useCase = new GetConsoleLogsUseCase(dispatcher, collector);

            var result = useCase.ExecuteAsync(50, false, true, true, true, CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("msg1", result[0].message);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_PassesAllParametersToCollector()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var collector = new SpyConsoleLogCollector(new List<ConsoleLogEntry>());
            var useCase = new GetConsoleLogsUseCase(dispatcher, collector);

            useCase.ExecuteAsync(25, true, false, true, false, CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, collector.GetLogsCallCount);
            Assert.AreEqual(25, collector.LastCount);
            Assert.IsTrue(collector.LastIncludeStackTrace);
            Assert.IsFalse(collector.LastShowLog);
            Assert.IsTrue(collector.LastShowWarning);
            Assert.IsFalse(collector.LastShowError);
        }
    }
}
