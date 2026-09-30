using System;
using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    // Keeps test run state in SessionState so that it survives domain reloads during a run.
    internal sealed class TestResultStore : ITestResultStore
    {
        private readonly string _resultJsonKey;
        private readonly string _pendingResultsJsonKey;

        // The keys are injected so that tests can use their own keys without touching the state of an ongoing run.
        public TestResultStore(string resultJsonKey, string pendingResultsJsonKey)
        {
            _resultJsonKey = resultJsonKey;
            _pendingResultsJsonKey = pendingResultsJsonKey;
        }

        // Pending results exist exactly while a run is pending: MarkPending writes them and StoreResult erases them.
        public bool IsPending => !string.IsNullOrEmpty(SessionState.GetString(_pendingResultsJsonKey, ""));

        public void MarkPending()
        {
            SessionState.SetString(_resultJsonKey, "");
            SavePendingResults(Array.Empty<TestResultItem>());
        }

        public void StoreResult(string json)
        {
            SessionState.SetString(_resultJsonKey, json);
            SessionState.EraseString(_pendingResultsJsonKey);
        }

        public string GetResult()
        {
            return SessionState.GetString(_resultJsonKey, "");
        }

        public void SavePendingResults(IReadOnlyList<TestResultItem> results)
        {
            var entries = new List<TestResultEntry>(results.Count);
            foreach (var item in results)
            {
                entries.Add(new TestResultEntry(item.Name, item.Status, item.Duration, item.Message));
            }

            SessionState.SetString(_pendingResultsJsonKey, JsonUtility.ToJson(new PendingResults(entries)));
        }

        public IReadOnlyList<TestResultItem> LoadPendingResults()
        {
            var results = new List<TestResultItem>();
            var json = SessionState.GetString(_pendingResultsJsonKey, "");
            if (string.IsNullOrEmpty(json))
            {
                return results;
            }

            foreach (var entry in JsonUtility.FromJson<PendingResults>(json).results)
            {
                results.Add(new TestResultItem(entry.name, entry.status, entry.duration, entry.message));
            }

            return results;
        }

        internal void Clear()
        {
            SessionState.EraseString(_resultJsonKey);
            SessionState.EraseString(_pendingResultsJsonKey);
        }

        [Serializable]
        private sealed class PendingResults
        {
            public List<TestResultEntry> results;

            public PendingResults(List<TestResultEntry> results)
            {
                this.results = results;
            }
        }
    }
}
