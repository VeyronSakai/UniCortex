using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetTimelineTimeUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly ITimelineOperations _operations;

        public SetTimelineTimeUseCase(IMainThreadDispatcher dispatcher, ITimelineOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(int instanceId, double time,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SetTimelineTime(instanceId, time), cancellationToken);
        }
    }
}
