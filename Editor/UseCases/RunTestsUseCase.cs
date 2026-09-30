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

        // Starts the run and returns once it has started; the results are obtained from GET /tests/result.
        public async Task ExecuteAsync(RunTestsRequest request, CancellationToken cancellationToken)
        {
            await _dispatcher.RunOnMainThreadAsync(() =>
            {
                if (_editorApplication.IsPlaying)
                {
                    throw new PlayModeException("Cannot run tests during play mode.");
                }
            }, cancellationToken);

            await _testRunner.StartAsync(request, cancellationToken);
        }
    }
}
