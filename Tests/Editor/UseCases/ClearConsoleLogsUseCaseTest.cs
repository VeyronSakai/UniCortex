using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class ClearConsoleLogsUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsClear_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var collector = new SpyConsoleLogCollector();
            var useCase = new ClearConsoleLogsUseCase(dispatcher, collector);

            useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, collector.ClearCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
