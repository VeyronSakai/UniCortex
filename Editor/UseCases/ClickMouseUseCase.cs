using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    // Presses a mouse button at the position, keeps it pressed for holdDuration seconds, and releases it.
    // Completes after the release has been processed, so the next request sees the result.
    internal sealed class ClickMouseUseCase
    {
        private readonly PlayerLoopRunner _runner;
        private readonly PointerPositionResolver _resolver;
        private readonly IInputOperations _operations;
        private readonly ITime _time;

        public ClickMouseUseCase(PlayerLoopRunner runner, PointerPositionResolver resolver,
            IInputOperations operations, ITime time)
        {
            _runner = runner;
            _resolver = resolver;
            _operations = operations;
            _time = time;
        }

        public async Task<MouseResponse> ExecuteAsync(PointerPosition position, string button,
            float holdDuration, CancellationToken cancellationToken = default)
        {
            if (holdDuration < 0f)
            {
                throw new ArgumentException("holdDuration must be 0 or greater.");
            }

            var (x, y) = await _resolver.ResolveAsync(position, cancellationToken);

            // Keep the physical mouse from moving the pointer until the release has been processed.
            _operations.BlockPhysicalMouse();
            try
            {
                // Each step runs in a later frame than the previous one.
                var pressedAt = await _runner.RunAsync(() => Press(x, y, button), cancellationToken);

                var released = false;
                while (!released)
                {
                    released = await _runner.RunAsync(
                        () => ReleaseIfHeld(x, y, button, pressedAt, holdDuration), cancellationToken);
                }

                await _runner.WaitForInputProcessedAsync(cancellationToken);
            }
            finally
            {
                _operations.UnblockPhysicalMouse();
            }

            return new MouseResponse(true, x, y);
        }

        // Presses the button and returns the time of the frame.
        private double Press(float x, float y, string button)
        {
            _operations.PressMouseButton(x, y, button);
            return _time.UnscaledTime;
        }

        // Releases the button when holdDuration seconds have passed since pressedAt, and returns whether it did.
        private bool ReleaseIfHeld(float x, float y, string button, double pressedAt, float holdDuration)
        {
            if (_time.UnscaledTime - pressedAt < holdDuration)
            {
                return false;
            }

            _operations.ReleaseMouseButton(x, y, button);
            return true;
        }
    }
}
