using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyTestRunner : ITestRunner
    {
        public int StartCallCount { get; private set; }
        public RunTestsRequest LastRequest { get; private set; }
        public string LastTestMode => LastRequest?.testMode;

        public Task StartAsync(RunTestsRequest request, CancellationToken cancellationToken)
        {
            StartCallCount++;
            LastRequest = request;
            return Task.CompletedTask;
        }
    }
}
