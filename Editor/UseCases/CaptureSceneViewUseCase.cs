using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class CaptureSceneViewUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorWindowOperations _windowOperations;
        private readonly ICaptureOperations _captureOperations;

        public CaptureSceneViewUseCase(IMainThreadDispatcher dispatcher, IEditorWindowOperations windowOperations,
            ICaptureOperations captureOperations)
        {
            _dispatcher = dispatcher;
            _windowOperations = windowOperations;
            _captureOperations = captureOperations;
        }

        public async Task<byte[]> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            // The Scene View camera is only set up while the Scene View draws itself (e.g. it stays at the
            // origin after a domain reload while hidden behind the Game View), so bring it to the front first.
            await _dispatcher.RunOnMainThreadAsync(() => _windowOperations.FocusSceneView(), cancellationToken);

            // Capture on a later main thread tick so that the Scene View has been drawn after gaining focus.
            return await _dispatcher.RunOnMainThreadAsync(
                () => _captureOperations.CaptureSceneView(), cancellationToken);
        }
    }
}
