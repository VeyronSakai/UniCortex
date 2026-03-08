using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class RunTestsUseCase
    {
        private readonly ITestRunner _testRunner;
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorApplication _editorApplication;

        public RunTestsUseCase(ITestRunner testRunner, IMainThreadDispatcher dispatcher,
            IEditorApplication editorApplication)
        {
            _testRunner = testRunner;
            _dispatcher = dispatcher;
            _editorApplication = editorApplication;
        }

        public async Task<RunTestsResponse> ExecuteAsync(RunTestsRequest request,
            CancellationToken cancellationToken)
        {
            await _dispatcher.RunOnMainThreadAsync(() =>
            {
                if (_editorApplication.IsPlaying)
                {
                    throw new PlayModeException("Cannot run tests during play mode.");
                }
            }, cancellationToken);

            var items = await _testRunner.RunTestsAsync(request, cancellationToken);

            var passed = items.Count(i => i.Status == "Passed");
            var failed = items.Count(i => i.Status == "Failed");
            var skipped = items.Count(i => i.Status != "Passed" && i.Status != "Failed");

            var results = new List<TestResultEntry>(items.Count);
            foreach (var item in items)
            {
                results.Add(new TestResultEntry(item.Name, item.Status, item.Duration, item.Message));
            }

            return new RunTestsResponse(passed, failed, skipped, results);
        }
    }
}
