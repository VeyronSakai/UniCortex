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

        private IReadOnlyList<TestResultItem> _results = new List<TestResultItem>();

        public void SetResults(IReadOnlyList<TestResultItem> results)
        {
            _results = results;
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
