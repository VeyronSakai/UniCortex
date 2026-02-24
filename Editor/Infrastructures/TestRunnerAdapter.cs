using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class TestRunnerAdapter : ITestRunner
    {
        private readonly IMainThreadDispatcher _dispatcher;

        public TestRunnerAdapter(IMainThreadDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task<IReadOnlyList<TestResultItem>> RunTestsAsync(string testMode, string nameFilter,
            CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<IReadOnlyList<TestResultItem>>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            await using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken)))
            {
                await _dispatcher.RunOnMainThreadAsync(() =>
                {
                    var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
                    var callbacks = new TestCallbacks(tcs);
                    testRunnerApi.RegisterCallbacks(callbacks);

                    var filter = new Filter
                    {
                        testMode = testMode == "PlayMode" ? TestMode.PlayMode : TestMode.EditMode,
                    };

                    if (!string.IsNullOrEmpty(nameFilter))
                    {
                        filter.testNames = new[] { nameFilter };
                    }

                    Debug.Log($"[UniCortex] Running tests: mode={testMode}, filter={nameFilter}");
                    testRunnerApi.Execute(new ExecutionSettings(filter));
                }, cancellationToken);

                return await tcs.Task;
            }
        }

        private sealed class TestCallbacks : ICallbacks
        {
            private readonly TaskCompletionSource<IReadOnlyList<TestResultItem>> _tcs;
            private readonly List<TestResultItem> _results = new();

            public TestCallbacks(TaskCompletionSource<IReadOnlyList<TestResultItem>> tcs)
            {
                _tcs = tcs;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                _tcs.TrySetResult(_results);
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.HasChildren)
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
}
