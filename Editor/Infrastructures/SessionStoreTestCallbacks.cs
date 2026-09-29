using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class SessionStoreTestCallbacks : ICallbacks
    {
        private readonly TestRunnerApi _testRunnerApi;
        private readonly TestResultStore _store;
        private readonly List<TestResultItem> _results;

        public SessionStoreTestCallbacks(TestRunnerApi testRunnerApi = null, TestResultStore store = null)
        {
            _testRunnerApi = testRunnerApi;
            _store = store ?? TestResultStore.Default;

            // A domain reload in the middle of a run (e.g. entering Play Mode) discards these callbacks,
            // and new ones are registered afterwards. Carry over the results reported before the reload.
            _results = _store.LoadPartialResults();
            AssemblyReloadEvents.beforeAssemblyReload += SavePartialResults;
        }

        internal void SavePartialResults()
        {
            _store.SavePartialResults(_results);
        }

        internal IReadOnlyList<TestResultItem> Results => _results;

        public void RunStarted(ITestAdaptor testsToRun)
        {
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            AssemblyReloadEvents.beforeAssemblyReload -= SavePartialResults;

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
            _testRunnerApi?.UnregisterCallbacks(this);
            Debug.Log("[UniCortex] Test results stored in SessionState");
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
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
