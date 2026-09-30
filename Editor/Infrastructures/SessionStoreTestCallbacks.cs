using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    // Records the results of runs started through POST /tests/run into the store, which GET /tests/result reads.
    // Registered once per domain and kept registered, so it also receives events of other runs
    // (e.g. started from the Test Runner window); those are ignored because no run is pending then.
    internal sealed class SessionStoreTestCallbacks : ICallbacks
    {
        private readonly ITestResultStore _store;
        private readonly List<TestResultItem> _results = new();

        public SessionStoreTestCallbacks(ITestResultStore store)
        {
            _store = store;

            // A domain reload in the middle of a run (e.g. entering Play Mode) replaces these callbacks with new ones.
            // Carry over the results reported before the reload.
            _results.AddRange(_store.LoadPendingResults());
            AssemblyReloadEvents.beforeAssemblyReload += SavePendingResults;
        }

        internal IReadOnlyList<TestResultItem> Results => _results;

        internal void SavePendingResults()
        {
            if (!_store.IsPending)
            {
                return;
            }

            _store.SavePendingResults(_results);
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            if (!_store.IsPending)
            {
                return;
            }

            // Start from the store rather than clearing, so this is also correct if the Test Runner reports
            // the start again after a domain reload.
            _results.Clear();
            _results.AddRange(_store.LoadPendingResults());
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            if (!_store.IsPending)
            {
                return;
            }

            var entries = new List<TestResultEntry>(_results.Count);
            int passed = 0, failed = 0, skipped = 0;
            foreach (var item in _results)
            {
                entries.Add(new TestResultEntry(item.Name, item.Status, item.Duration, item.Message));
                switch (item.Status)
                {
                    case TestStatuses.Passed:
                        passed++;
                        break;
                    case TestStatuses.Failed:
                        failed++;
                        break;
                    default:
                        skipped++;
                        break;
                }
            }

            var response = new RunTestsResponse(passed, failed, skipped, entries);
            _store.StoreResult(JsonUtility.ToJson(response));
            _results.Clear();
            Debug.Log("[UniCortex] Test results stored in SessionState");
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (!_store.IsPending)
            {
                return;
            }

            // HasChildren: skip parent nodes that contain child test results
            // IsSuite: skip container nodes (assemblies, namespaces, classes, folders, etc.)
            //          When a nameFilter excludes all tests, suite nodes are reported with
            //          HasChildren == false, so the IsSuite check is also required.
            if (result.HasChildren || result.Test.IsSuite)
            {
                return;
            }

            var status = result.TestStatus.ToString();
            var duration = (float)result.Duration;
            var message = result.Message ?? "";
            _results.Add(new TestResultItem(result.Name, status, duration, message));
        }
    }
}
