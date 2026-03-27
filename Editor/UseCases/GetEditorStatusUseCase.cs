using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetEditorStatusUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorApplication _editorApplication;

        public GetEditorStatusUseCase(IMainThreadDispatcher dispatcher, IEditorApplication editorApplication)
        {
            _dispatcher = dispatcher;
            _editorApplication = editorApplication;
        }

        public async Task<GetEditorStatusResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            var (isPlaying, isPaused, screenWidth, screenHeight) = await _dispatcher.RunOnMainThreadAsync(
                () => (_editorApplication.IsPlaying, _editorApplication.IsPaused,
                    _editorApplication.ScreenWidth, _editorApplication.ScreenHeight), cancellationToken);
            return new GetEditorStatusResponse(isPlaying, isPaused, screenWidth, screenHeight);
        }
    }
}
