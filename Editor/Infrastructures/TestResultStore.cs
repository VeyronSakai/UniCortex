using System;
using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    // Keeps test run state in SessionState so that it survives domain reloads during a run.
    internal sealed class TestResultStore
    {
        internal static readonly TestResultStore Default = new("UniCortex.");

        private readonly string _pendingKey;
        private readonly string _resultJsonKey;
        private readonly string _partialResultsJsonKey;

        // The key prefix lets tests use their own keys without touching the state of an ongoing run.
        internal TestResultStore(string keyPrefix)
        {
            _pendingKey = keyPrefix + "TestRunPending";
            _resultJsonKey = keyPrefix + "TestResultJson";
            _partialResultsJsonKey = keyPrefix + "TestPartialResultsJson";
        }

        internal bool IsPending => SessionState.GetBool(_pendingKey, false);

        internal void MarkPending()
        {
            SessionState.SetBool(_pendingKey, true);
            SessionState.SetString(_resultJsonKey, "");
            SessionState.EraseString(_partialResultsJsonKey);
        }

        internal void StoreResult(string json)
        {
            SessionState.SetString(_resultJsonKey, json);
            SessionState.EraseString(_partialResultsJsonKey);
            SessionState.SetBool(_pendingKey, false);
        }

        internal string GetResult()
        {
            return SessionState.GetString(_resultJsonKey, "");
        }

        // Results of tests that finished before a domain reload in the middle of a run.
        internal void SavePartialResults(IReadOnlyList<TestResultItem> results)
        {
            var entries = new List<TestResultEntry>(results.Count);
            foreach (var item in results)
            {
                entries.Add(new TestResultEntry(item.Name, item.Status, item.Duration, item.Message));
            }

            SessionState.SetString(_partialResultsJsonKey, JsonUtility.ToJson(new PartialResults(entries)));
        }

        internal List<TestResultItem> LoadPartialResults()
        {
            var results = new List<TestResultItem>();
            var json = SessionState.GetString(_partialResultsJsonKey, "");
            if (string.IsNullOrEmpty(json))
            {
                return results;
            }

            foreach (var entry in JsonUtility.FromJson<PartialResults>(json).results)
            {
                results.Add(new TestResultItem(entry.name, entry.status, entry.duration, entry.message));
            }

            return results;
        }

        internal void Clear()
        {
            SessionState.EraseBool(_pendingKey);
            SessionState.EraseString(_resultJsonKey);
            SessionState.EraseString(_partialResultsJsonKey);
        }

        [Serializable]
        private sealed class PartialResults
        {
            public List<TestResultEntry> results;

            public PartialResults(List<TestResultEntry> results)
            {
                this.results = results;
            }
        }
    }
}
