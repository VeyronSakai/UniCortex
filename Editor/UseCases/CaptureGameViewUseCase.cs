using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class CaptureGameViewUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorApplication _editorApplication;
        private readonly IEditorWindowOperations _windowOperations;
        private readonly ICaptureOperations _captureOperations;
        private readonly IPlayModeViewOperations _playModeViewOperations;

        public CaptureGameViewUseCase(IMainThreadDispatcher dispatcher, IEditorApplication editorApplication,
            IEditorWindowOperations windowOperations, ICaptureOperations captureOperations,
            IPlayModeViewOperations playModeViewOperations)
        {
            _dispatcher = dispatcher;
            _editorApplication = editorApplication;
            _windowOperations = windowOperations;
            _captureOperations = captureOperations;
            _playModeViewOperations = playModeViewOperations;
        }

        // deviceFrame draws the device frame of the Simulator view. When null, the frame is drawn in the
        // Simulator view only.
        public async Task<byte[]> ExecuteAsync(bool drawSafeArea, bool? deviceFrame,
            CancellationToken cancellationToken = default)
        {
            var drawDeviceFrame = await _dispatcher.RunOnMainThreadAsync(() =>
            {
                if (!_editorApplication.IsPlaying)
                {
                    throw new InvalidOperationException(
                        "Game View capture is only available in Play Mode. Enter Play Mode first, " +
                        "or use Scene View capture in Edit Mode.");
                }

                var isSimulatorView = _playModeViewOperations.GetViewType() == PlayModeViewTypes.SimulatorView;
                if (deviceFrame == true && !isSimulatorView)
                {
                    throw new InvalidOperationException(
                        "The device frame is only available in the Simulator view. " +
                        "Switch the Play Mode window to the Simulator view first, or omit deviceFrame.");
                }

                // The Game View (or the Simulator view) only renders while it is visible,
                // so open it if needed and bring it to the front.
                _windowOperations.OpenGameView();

                return (deviceFrame ?? true) && isSimulatorView;
            }, cancellationToken);

            // Capture on a later main thread tick so that the Game View has been drawn after gaining focus.
            return await _dispatcher.RunOnMainThreadAsync(() =>
            {
                var safeArea = drawSafeArea ? _playModeViewOperations.GetScreenSafeArea() : null;
                return _captureOperations.CaptureGameView(safeArea, drawDeviceFrame);
            }, cancellationToken);
        }
    }
}
