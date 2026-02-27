using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyTestRunner : ITestRunner
    {
        public int RunTestsCallCount { get; private set; }
        public string LastTestMode { get; private set; }
        public string LastNameFilter { get; private set; }

        private readonly IReadOnlyList<TestResultItem> _results;

        public SpyTestRunner(IReadOnlyList<TestResultItem> results = null)
        {
            _results = results ?? new List<TestResultItem>();
        }

        public Task<IReadOnlyList<TestResultItem>> RunTestsAsync(string testMode, string nameFilter,
            CancellationToken cancellationToken)
        {
            RunTestsCallCount++;
            LastTestMode = testMode;
            LastNameFilter = nameFilter;
            return Task.FromResult(_results);
        }
    }
}
