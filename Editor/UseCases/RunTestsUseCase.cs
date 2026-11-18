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
        private readonly IEditorSceneManager _sceneManager;

        public RunTestsUseCase(ITestRunner testRunner, IMainThreadDispatcher dispatcher,
            IEditorApplication editorApplication, IEditorSceneManager sceneManager)
        {
            _testRunner = testRunner;
            _dispatcher = dispatcher;
            _editorApplication = editorApplication;
            _sceneManager = sceneManager;
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

                // The Test Runner would ask to save scenes with unsaved changes in a dialog that blocks the Editor.
                var dirtyScenePaths = _sceneManager.GetDirtyScenePaths();
                if (dirtyScenePaths.Count > 0)
                {
                    throw new UnsavedSceneChangesException("Cannot run tests", dirtyScenePaths);
                }
            }, cancellationToken);

            await _testRunner.StartAsync(request, cancellationToken);
        }
    }
}
