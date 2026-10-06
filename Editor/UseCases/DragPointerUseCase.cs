using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    // Presses at the start, keeps the button pressed for holdDuration seconds, moves along a straight line to the
    // end over duration seconds (one move per frame), and releases at the end.
    // Completes after the release has been processed, so the next request sees the result.
    internal sealed class DragPointerUseCase
    {
        public const float DefaultDuration = 0.2f;

        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPlayerLoopDispatcher _playerLoopDispatcher;
        private readonly PointerPositionResolver _resolver;
        private readonly IInputOperations _operations;

        public DragPointerUseCase(IMainThreadDispatcher dispatcher, IPlayerLoopDispatcher playerLoopDispatcher,
            PointerPositionResolver resolver, IInputOperations operations)
        {
            _dispatcher = dispatcher;
            _playerLoopDispatcher = playerLoopDispatcher;
            _resolver = resolver;
            _operations = operations;
        }

        public async Task<DragPointerResponse> ExecuteAsync(PointerPosition start, PointerPosition end,
            string button, float duration, float holdDuration, CancellationToken cancellationToken = default)
        {
            if (duration < 0f)
            {
                throw new ArgumentException("duration must be 0 or greater.");
            }

            if (holdDuration < 0f)
            {
                throw new ArgumentException("holdDuration must be 0 or greater.");
            }

            var (fromX, fromY) = await _resolver.ResolveAsync(start, cancellationToken);
            var (toX, toY) = await _resolver.ResolveAsync(end, cancellationToken);

            // Frame 0 presses. Later frames wait until holdDuration seconds have passed, then move toward the end
            // by the time since the hold ended, until the end is reached. The next frame releases, and the last
            // frame waits so that the release is processed in it.
            var reachedEnd = false;
            var released = false;
            bool Step(int frame, double elapsed)
            {
                if (frame == 0)
                {
                    _operations.PressMouseButton(fromX, fromY, button);
                    return true;
                }

                if (!reachedEnd)
                {
                    var moveTime = elapsed - holdDuration;
                    if (moveTime <= 0d)
                    {
                        return true;
                    }

                    var t = duration <= 0f ? 1f : (float)Math.Min(1d, moveTime / duration);
                    _operations.MoveMouse(fromX + (toX - fromX) * t, fromY + (toY - fromY) * t);
                    reachedEnd = t >= 1f;
                    return true;
                }

                if (!released)
                {
                    _operations.ReleaseMouseButton(toX, toY, button);
                    released = true;
                    return true;
                }

                return false;
            }

            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _playerLoopDispatcher.RunEachFrameAsync(Step, cancellationToken), cancellationToken);
            await task;

            return new DragPointerResponse(true, fromX, fromY, toX, toY);
        }
    }
}
