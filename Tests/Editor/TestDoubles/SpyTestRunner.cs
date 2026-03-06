using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyTestRunner : ITestRunner
    {
        public int RunTestsCallCount { get; private set; }
        public RunTestsRequest LastRequest { get; private set; }
        public string LastTestMode => LastRequest?.testMode;

        private readonly IReadOnlyList<TestResultItem> _results;

        public SpyTestRunner(IReadOnlyList<TestResultItem> results = null)
        {
            _results = results ?? new List<TestResultItem>();
        }

        public Task<IReadOnlyList<TestResultItem>> RunTestsAsync(RunTestsRequest request,
            CancellationToken cancellationToken)
        {
            RunTestsCallCount++;
            LastRequest = request;
            return Task.FromResult(_results);
        }
    }
}
