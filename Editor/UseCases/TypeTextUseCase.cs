using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    // Types the text into the focused text field in one frame.
    // Completes after the text has been processed, so the next request sees the result.
    internal sealed class TypeTextUseCase
    {
        internal const string TextRequiredMessage = "text is required.";

        private readonly PlayerLoopRunner _runner;
        private readonly IInputOperations _operations;

        public TypeTextUseCase(PlayerLoopRunner runner, IInputOperations operations)
        {
            _runner = runner;
            _operations = operations;
        }

        public async Task ExecuteAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException(TextRequiredMessage);
            }

            // Keep the physical keyboard from typing between the simulated characters until they have been processed.
            _operations.BlockPhysicalKeyboard();
            try
            {
                await _runner.RunAsync(() => _operations.TypeText(text), cancellationToken);
                await _runner.WaitForInputProcessedAsync(cancellationToken);
            }
            finally
            {
                _operations.UnblockPhysicalKeyboard();
            }
        }
    }
}
