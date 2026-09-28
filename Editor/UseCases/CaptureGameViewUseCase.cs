using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class CaptureGameViewUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorApplication _editorApplication;
        private readonly IEditorWindowOperations _windowOperations;
        private readonly ICaptureOperations _captureOperations;

        public CaptureGameViewUseCase(IMainThreadDispatcher dispatcher, IEditorApplication editorApplication,
            IEditorWindowOperations windowOperations, ICaptureOperations captureOperations)
        {
            _dispatcher = dispatcher;
            _editorApplication = editorApplication;
            _windowOperations = windowOperations;
            _captureOperations = captureOperations;
        }

        public async Task<byte[]> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() =>
            {
                if (!_editorApplication.IsPlaying)
                {
                    throw new InvalidOperationException(
                        "Game View capture is only available in Play Mode. Enter Play Mode first, " +
                        "or use Scene View capture in Edit Mode.");
                }

                // The Game View only renders while it is visible, so open it if needed and bring it to the front.
                _windowOperations.OpenGameView();
            }, cancellationToken);

            // Capture on a later main thread tick so that the Game View has been drawn after gaining focus.
            return await _dispatcher.RunOnMainThreadAsync(
                () => _captureOperations.CaptureGameView(), cancellationToken);
        }
    }
}
