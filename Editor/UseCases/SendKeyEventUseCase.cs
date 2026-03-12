using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SendKeyEventUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IInputSimulationOperations _operations;

        public SendKeyEventUseCase(IMainThreadDispatcher dispatcher, IInputSimulationOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(string keyName, string eventType, CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SendKeyEvent(keyName, eventType), cancellationToken);
        }
    }
}
