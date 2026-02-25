using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class SessionStoreTestCallbacks : ICallbacks
    {
        private readonly List<TestResultItem> _results = new();

        internal IReadOnlyList<TestResultItem> Results => _results;

        public void RunStarted(ITestAdaptor testsToRun)
        {
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            var entries = new List<TestResultEntry>(_results.Count);
            int passed = 0, failed = 0, skipped = 0;
            foreach (var item in _results)
            {
                entries.Add(new TestResultEntry(item.Name, item.Status, item.Duration, item.Message));
                switch (item.Status)
                {
                    case "Passed":
                        passed++;
                        break;
                    case "Failed":
                        failed++;
                        break;
                    default:
                        skipped++;
                        break;
                }
            }

            var response = new RunTestsResponse(passed, failed, skipped, entries);
            TestResultStore.StoreResult(JsonUtility.ToJson(response));
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
