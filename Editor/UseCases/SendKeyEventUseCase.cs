using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SendKeyEventUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IInputOperations _operations;

        public SendKeyEventUseCase(IMainThreadDispatcher dispatcher,
            IInputOperations operations)
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
