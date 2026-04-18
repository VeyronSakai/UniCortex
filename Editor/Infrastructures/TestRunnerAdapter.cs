using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
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

        public async Task<IReadOnlyList<TestResultItem>> RunTestsAsync(RunTestsRequest request,
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
                    var callbacks = new TestCallbacks(testRunnerApi, tcs);
                    testRunnerApi.RegisterCallbacks(callbacks);

                    var filter = new Filter
                    {
                        testMode = request.testMode == TestModes.PlayMode ? TestMode.PlayMode : TestMode.EditMode,
                    };

                    if (request.testNames != null && request.testNames.Count > 0)
                    {
                        filter.testNames = request.testNames.ToArray();
                    }

                    if (request.groupNames != null && request.groupNames.Count > 0)
                    {
                        filter.groupNames = request.groupNames.ToArray();
                    }

                    if (request.categoryNames != null && request.categoryNames.Count > 0)
                    {
                        filter.categoryNames = request.categoryNames.ToArray();
                    }

                    if (request.assemblyNames != null && request.assemblyNames.Count > 0)
                    {
                        filter.assemblyNames = request.assemblyNames.ToArray();
                    }

                    // Save all open scenes before running tests to prevent
                    // "Scene(s) Have Been Modified" dialog from Unity Test Runner's SaveModifiedSceneTask.
                    // Skip during play mode as SaveOpenScenes throws InvalidOperationException.
                    if (!UnityEditor.EditorApplication.isPlaying)
                    {
                        if (!UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes())
                        {
                            const string message =
                                "[UniCortex] Failed to save open scenes before running tests. Aborting test run.";
                            Debug.LogError(message);
                            throw new System.InvalidOperationException(message);
                        }
                    }

                    Debug.Log($"[UniCortex] Running tests: mode={request.testMode}");
                    testRunnerApi.Execute(new ExecutionSettings(filter));
                }, cancellationToken);

                return await tcs.Task;
            }
        }

        private sealed class TestCallbacks : ICallbacks
        {
            private readonly TestRunnerApi _testRunnerApi;
            private readonly TaskCompletionSource<IReadOnlyList<TestResultItem>> _tcs;
            private readonly SessionStoreTestCallbacks _inner;

            public TestCallbacks(TestRunnerApi testRunnerApi, TaskCompletionSource<IReadOnlyList<TestResultItem>> tcs)
            {
                _testRunnerApi = testRunnerApi;
                _tcs = tcs;
                _inner = new SessionStoreTestCallbacks();
            }

            public void RunStarted(ITestAdaptor testsToRun) => _inner.RunStarted(testsToRun);

            public void TestStarted(ITestAdaptor test) => _inner.TestStarted(test);

            public void TestFinished(ITestResultAdaptor result) => _inner.TestFinished(result);

            public void RunFinished(ITestResultAdaptor result)
            {
                _inner.RunFinished(result);
                _testRunnerApi.UnregisterCallbacks(this);
                _tcs.TrySetResult(_inner.Results);
            }
        }
    }
}
