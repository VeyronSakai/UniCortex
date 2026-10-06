using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    // Presses at the start, moves along a straight line to the end over duration seconds (one move per frame), and
    // releases at the end.
    // Completes after the release has been processed, so the next request sees the result.
    internal sealed class DragMouseUseCase
    {
        public const float DefaultDuration = 0.2f;

        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPlayerLoopDispatcher _playerLoopDispatcher;
        private readonly PointerPositionResolver _resolver;
        private readonly IInputOperations _operations;

        public DragMouseUseCase(IMainThreadDispatcher dispatcher, IPlayerLoopDispatcher playerLoopDispatcher,
            PointerPositionResolver resolver, IInputOperations operations)
        {
            _dispatcher = dispatcher;
            _playerLoopDispatcher = playerLoopDispatcher;
            _resolver = resolver;
            _operations = operations;
        }

        public async Task<DragMouseResponse> ExecuteAsync(PointerPosition start, PointerPosition end,
            string button, float duration, CancellationToken cancellationToken = default)
        {
            if (duration < 0f)
            {
                throw new ArgumentException("duration must be 0 or greater.");
            }

            var (fromX, fromY) = await _resolver.ResolveAsync(start, cancellationToken);
            var (toX, toY) = await _resolver.ResolveAsync(end, cancellationToken);

            var sequence = new DragSequence(_operations, fromX, fromY, toX, toY, button, duration);
            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _playerLoopDispatcher.RunEachFrameAsync(sequence.Advance, cancellationToken),
                cancellationToken);
            await task;

            return new DragMouseResponse(true, fromX, fromY, toX, toY);
        }

        // The steps of one drag, run once per frame.
        private sealed class DragSequence
        {
            private readonly IInputOperations _operations;
            private readonly float _fromX;
            private readonly float _fromY;
            private readonly float _toX;
            private readonly float _toY;
            private readonly string _button;
            private readonly float _duration;
            private bool _reachedEnd;
            private bool _released;

            public DragSequence(IInputOperations operations, float fromX, float fromY, float toX, float toY,
                string button, float duration)
            {
                _operations = operations;
                _fromX = fromX;
                _fromY = fromY;
                _toX = toX;
                _toY = toY;
                _button = button;
                _duration = duration;
            }

            // Runs the step of the given frame and returns false when the drag is done.
            // Frame 0 presses. Later frames move toward the end by the time since the press, until the end is
            // reached. The next frame releases, and the last frame waits so that the release is processed in it.
            public bool Advance(int frame, double elapsed)
            {
                if (frame == 0)
                {
                    _operations.PressMouseButton(_fromX, _fromY, _button);
                    return true;
                }

                if (!_reachedEnd)
                {
                    var t = _duration <= 0f ? 1f : (float)Math.Min(1d, elapsed / _duration);
                    _operations.MoveMouse(_fromX + (_toX - _fromX) * t, _fromY + (_toY - _fromY) * t);
                    _reachedEnd = t >= 1f;
                    return true;
                }

                if (!_released)
                {
                    _operations.ReleaseMouseButton(_toX, _toY, _button);
                    _released = true;
                    return true;
                }

                return false;
            }
        }
    }
}
