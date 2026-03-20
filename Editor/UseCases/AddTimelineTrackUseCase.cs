using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class AddTimelineTrackUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly ITimelineOperations _operations;

        public AddTimelineTrackUseCase(IMainThreadDispatcher dispatcher, ITimelineOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(int instanceId, string trackType, string trackName,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.AddTrack(instanceId, trackType, trackName), cancellationToken);
        }
    }
}
