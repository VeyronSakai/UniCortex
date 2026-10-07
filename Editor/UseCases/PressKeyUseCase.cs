using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    // Presses the keys together, keeps them pressed for holdDuration seconds, and releases them.
    // Completes after the release has been processed, so the next request sees the result.
    internal sealed class PressKeyUseCase
    {
        internal const string KeysRequiredMessage = "keys is required.";

        private readonly PlayerLoopRunner _runner;
        private readonly IInputOperations _operations;
        private readonly ITime _time;

        public PressKeyUseCase(PlayerLoopRunner runner, IInputOperations operations, ITime time)
        {
            _runner = runner;
            _operations = operations;
            _time = time;
        }

        public async Task ExecuteAsync(string[] keys, float holdDuration,
            CancellationToken cancellationToken = default)
        {
            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentException(KeysRequiredMessage);
            }

            if (holdDuration < 0f)
            {
                throw new ArgumentException("holdDuration must be 0 or greater.");
            }

            // Keep the physical keyboard from changing the keys until the release has been processed.
            _operations.BlockPhysicalKeyboard();
            try
            {
                // Each step runs in a later frame than the previous one.
                var pressedAt = await _runner.RunAsync(() => Press(keys), cancellationToken);

                var released = false;
                while (!released)
                {
                    released = await _runner.RunAsync(
                        () => ReleaseIfHeld(keys, pressedAt, holdDuration), cancellationToken);
                }

                await _runner.WaitForInputProcessedAsync(cancellationToken);
            }
            finally
            {
                _operations.UnblockPhysicalKeyboard();
            }
        }

        // Presses the keys and returns the time of the frame.
        private double Press(string[] keys)
        {
            _operations.PressKeys(keys);
            return _time.UnscaledTime;
        }

        // Releases the keys when holdDuration seconds have passed since pressedAt, and returns whether it did.
        private bool ReleaseIfHeld(string[] keys, double pressedAt, float holdDuration)
        {
            if (_time.UnscaledTime - pressedAt < holdDuration)
            {
                return false;
            }

            _operations.ReleaseKeys(keys);
            return true;
        }
    }
}
