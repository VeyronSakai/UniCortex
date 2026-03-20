using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetTimelineInfoUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly ITimelineOperations _operations;

        public GetTimelineInfoUseCase(IMainThreadDispatcher dispatcher, ITimelineOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<TimelineInfoResponse> ExecuteAsync(int instanceId,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetTimelineInfo(instanceId), cancellationToken);
        }
    }
}
