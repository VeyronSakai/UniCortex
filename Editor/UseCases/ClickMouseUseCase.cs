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

            var sequence = new ClickSequence(_operations, x, y, button, holdDuration);
            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _playerLoopDispatcher.RunEachFrameAsync(sequence.Advance, cancellationToken),
                cancellationToken);
            await task;

            return new MouseResponse(true, x, y);
        }

        // The steps of one click, run once per frame.
        private sealed class ClickSequence
        {
            private readonly IInputOperations _operations;
            private readonly float _x;
            private readonly float _y;
            private readonly string _button;
            private readonly float _holdDuration;
            private bool _released;

            public ClickSequence(IInputOperations operations, float x, float y, string button, float holdDuration)
            {
                _operations = operations;
                _x = x;
                _y = y;
                _button = button;
                _holdDuration = holdDuration;
            }

            // Runs the step of the given frame and returns false when the click is done.
            // Frame 0 presses. The first later frame at least holdDuration seconds after the press releases, and
            // the next frame waits so that the release is processed in it.
            public bool Advance(int frame, double elapsed)
            {
                if (frame == 0)
                {
                    _operations.PressMouseButton(_x, _y, _button);
                    return true;
                }

                if (_released)
                {
                    return false;
                }

                if (elapsed >= _holdDuration)
                {
                    _operations.ReleaseMouseButton(_x, _y, _button);
                    _released = true;
                }

                return true;
            }
        }
    }
}
