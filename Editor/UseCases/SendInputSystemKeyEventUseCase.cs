using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SendInputSystemKeyEventUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IInputSystemSimulationOperations _operations;

        public SendInputSystemKeyEventUseCase(IMainThreadDispatcher dispatcher,
            IInputSystemSimulationOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(string key, string eventType,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SendKeyEvent(key, eventType), cancellationToken);
        }
    }
}
