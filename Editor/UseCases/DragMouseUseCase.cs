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

        private readonly PlayerLoopRunner _runner;
        private readonly PointerPositionResolver _resolver;
        private readonly IInputOperations _operations;
        private readonly ITime _time;

        public DragMouseUseCase(PlayerLoopRunner runner, PointerPositionResolver resolver,
            IInputOperations operations, ITime time)
        {
            _runner = runner;
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
            var pressedAt = await _runner.RunAsync(() => Press(fromX, fromY, button), cancellationToken);

            var reachedEnd = false;
            while (!reachedEnd)
            {
                reachedEnd = await _runner.RunAsync(
                    () => MoveTowardEnd(fromX, fromY, toX, toY, pressedAt, duration), cancellationToken);
            }

            await _runner.RunAsync(() => _operations.ReleaseMouseButton(toX, toY, button), cancellationToken);

            await _runner.WaitForInputProcessedAsync(cancellationToken);

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
            // Fraction of the drag done: 0 at the press, 1 after duration seconds.
            var progress = duration <= 0f
                ? 1f
                : (float)Math.Min(1d, (_time.UnscaledTime - pressedAt) / duration);
            _operations.MoveMouse(Lerp(fromX, toX, progress), Lerp(fromY, toY, progress));
            return progress >= 1f;
        }

        private static float Lerp(float from, float to, float t)
        {
            return from + (to - from) * t;
        }
    }
}
