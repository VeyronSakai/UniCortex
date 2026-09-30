using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Interfaces
{
    // Test run state that must survive domain reloads during a run.
    internal interface ITestResultStore
    {
        // True between MarkPending and StoreResult.
        bool IsPending { get; }

        // Starts a run: clears the previous result and pending results.
        void MarkPending();

        // Finishes a run with its result JSON.
        void StoreResult(string json);

        // The last stored result JSON, or an empty string while a run is pending or before any run.
        string GetResult();

        // Results of tests that finished before a domain reload in the middle of a run.
        void SavePendingResults(IReadOnlyList<TestResultItem> results);

        IReadOnlyList<TestResultItem> LoadPendingResults();
    }
}
