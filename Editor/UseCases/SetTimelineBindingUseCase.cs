using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetTimelineBindingUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly ITimelineOperations _operations;

        public SetTimelineBindingUseCase(IMainThreadDispatcher dispatcher, ITimelineOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(int instanceId, int trackIndex, int targetInstanceId,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SetBinding(instanceId, trackIndex, targetInstanceId), cancellationToken);
        }
    }
}
