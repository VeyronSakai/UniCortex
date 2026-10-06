using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    // Presses at the start, moves along a straight line to the end over duration seconds (at most one move per
    // frame), and releases at the end.
    // Completes after the release has been processed, so the next request sees the result.
    internal sealed class DragMouseUseCase
    {
        public const float DefaultDuration = 0.2f;

        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPlayerLoopDispatcher _playerLoopDispatcher;
        private readonly PointerPositionResolver _resolver;
        private readonly IInputOperations _operations;
        private readonly ITime _time;

        public DragMouseUseCase(IMainThreadDispatcher dispatcher, IPlayerLoopDispatcher playerLoopDispatcher,
            PointerPositionResolver resolver, IInputOperations operations, ITime time)
        {
            _dispatcher = dispatcher;
            _playerLoopDispatcher = playerLoopDispatcher;
            _resolver = resolver;
            _operations = operations;
            _time = time;
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

            // Each step runs in a later frame than the previous one.
            var pressedAt = await RunInPlayerLoopAsync(() => Press(fromX, fromY, button), cancellationToken);

            var reachedEnd = false;
            while (!reachedEnd)
            {
                reachedEnd = await RunInPlayerLoopAsync(
                    () => MoveTowardEnd(fromX, fromY, toX, toY, pressedAt, duration), cancellationToken);
            }

            await RunInPlayerLoopAsync(() => _operations.ReleaseMouseButton(toX, toY, button), cancellationToken);

            // Wait one more frame so that the release is processed.
            await RunInPlayerLoopAsync(() => { }, cancellationToken);

            return new DragMouseResponse(true, fromX, fromY, toX, toY);
        }

        // Presses the button and returns the time of the frame.
        private double Press(float x, float y, string button)
        {
            _operations.PressMouseButton(x, y, button);
            return _time.UnscaledTime;
        }

        // Moves to the point on the line from the start to the end that matches the time since pressedAt, and
        // returns whether it is the end.
        private bool MoveTowardEnd(float fromX, float fromY, float toX, float toY, double pressedAt, float duration)
        {
            var t = duration <= 0f ? 1f : (float)Math.Min(1d, (_time.UnscaledTime - pressedAt) / duration);
            _operations.MoveMouse(fromX + (toX - fromX) * t, fromY + (toY - fromY) * t);
            return t >= 1f;
        }

        private async Task<T> RunInPlayerLoopAsync<T>(Func<T> func, CancellationToken cancellationToken)
        {
            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _playerLoopDispatcher.RunAsync(func, cancellationToken), cancellationToken);
            return await task;
        }

        private async Task RunInPlayerLoopAsync(Action action, CancellationToken cancellationToken)
        {
            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _playerLoopDispatcher.RunAsync(action, cancellationToken), cancellationToken);
            await task;
        }
    }
}
