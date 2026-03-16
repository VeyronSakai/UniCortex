using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class UnpauseUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorApplication _editorApplication;

        public UnpauseUseCase(IMainThreadDispatcher dispatcher, IEditorApplication editorApplication)
        {
            _dispatcher = dispatcher;
            _editorApplication = editorApplication;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() => _editorApplication.IsPaused = false, cancellationToken);
        }
    }
}
