using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    // In-memory ITestResultStore that mimics TestResultStore without touching SessionState.
    internal sealed class FakeTestResultStore : ITestResultStore
    {
        private List<TestResultItem> _pendingResults;

        public string Result { get; set; } = "";

        public bool IsPending => _pendingResults != null;

        public void MarkPending()
        {
            Result = "";
            _pendingResults = new List<TestResultItem>();
        }

        public void StoreResult(string json)
        {
            Result = json;
            _pendingResults = null;
        }

        public string GetResult()
        {
            return Result;
        }

        public void SavePendingResults(IReadOnlyList<TestResultItem> results)
        {
            _pendingResults = new List<TestResultItem>(results);
        }

        public IReadOnlyList<TestResultItem> LoadPendingResults()
        {
            return _pendingResults ?? new List<TestResultItem>();
        }
    }
}
