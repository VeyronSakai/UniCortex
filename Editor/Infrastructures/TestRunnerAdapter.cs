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
                    TestResultStore.MarkPending();

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
            private readonly SessionStoreTestCallbacks _inner = new();

            public TestCallbacks(TaskCompletionSource<IReadOnlyList<TestResultItem>> tcs)
            {
                _tcs = tcs;
            }

            public void RunStarted(ITestAdaptor testsToRun) => _inner.RunStarted(testsToRun);

            public void TestStarted(ITestAdaptor test) => _inner.TestStarted(test);

            public void TestFinished(ITestResultAdaptor result) => _inner.TestFinished(result);

            public void RunFinished(ITestResultAdaptor result)
            {
                _inner.RunFinished(result);
                _tcs.TrySetResult(_inner.Results);
            }
        }
    }
}
