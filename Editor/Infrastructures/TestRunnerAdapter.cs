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
        private readonly ITestResultStore _testResultStore;

        public TestRunnerAdapter(IMainThreadDispatcher dispatcher, ITestResultStore testResultStore)
        {
            _dispatcher = dispatcher;
            _testResultStore = testResultStore;
        }

        public Task StartAsync(RunTestsRequest request, CancellationToken cancellationToken)
        {
            return _dispatcher.RunOnMainThreadAsync(() =>
            {
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

                // Scenes are not saved here: RunTestsUseCase refuses to start while a scene has unsaved changes,
                // so the Test Runner does not show its "Scene(s) Have Been Modified" dialog.
                _testResultStore.MarkPending();

                Debug.Log($"[UniCortex] Running tests: mode={request.testMode}");
                var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
                testRunnerApi.Execute(new ExecutionSettings(filter));
            }, cancellationToken);
        }
    }
}
