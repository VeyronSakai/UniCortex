using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    // Presses a pointer button at the position, keeps it pressed for holdDuration seconds, and releases it.
    // Completes after the release has been processed, so the next request sees the result.
    internal sealed class ClickMouseUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPlayerLoopDispatcher _playerLoopDispatcher;
        private readonly PointerPositionResolver _resolver;
        private readonly IInputOperations _operations;

        public ClickMouseUseCase(IMainThreadDispatcher dispatcher, IPlayerLoopDispatcher playerLoopDispatcher,
            PointerPositionResolver resolver, IInputOperations operations)
        {
            _dispatcher = dispatcher;
            _playerLoopDispatcher = playerLoopDispatcher;
            _resolver = resolver;
            _operations = operations;
        }

        public async Task<MouseResponse> ExecuteAsync(PointerPosition position, string button,
            float holdDuration, CancellationToken cancellationToken = default)
        {
            if (holdDuration < 0f)
            {
                throw new ArgumentException("holdDuration must be 0 or greater.");
            }

            var (x, y) = await _resolver.ResolveAsync(position, cancellationToken);

            // Frame 0 presses. The first later frame at least holdDuration seconds after the press releases, and
            // the next frame waits so that the release is processed in it.
            var released = false;
            bool Step(int frame, double elapsed)
            {
                if (frame == 0)
                {
                    _operations.PressMouseButton(x, y, button);
                    return true;
                }

                if (released)
                {
                    return false;
                }

                if (elapsed >= holdDuration)
                {
                    _operations.ReleaseMouseButton(x, y, button);
                    released = true;
                }

                return true;
            }

            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _playerLoopDispatcher.RunEachFrameAsync(Step, cancellationToken), cancellationToken);
            await task;

            return new MouseResponse(true, x, y);
        }
    }
}
