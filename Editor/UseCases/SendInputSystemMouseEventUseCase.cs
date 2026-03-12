using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SendInputSystemMouseEventUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IInputSystemSimulationOperations _operations;

        public SendInputSystemMouseEventUseCase(IMainThreadDispatcher dispatcher,
            IInputSystemSimulationOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(float x, float y, int button, string eventType,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SendMouseEvent(x, y, button, eventType), cancellationToken);
        }
    }
}
