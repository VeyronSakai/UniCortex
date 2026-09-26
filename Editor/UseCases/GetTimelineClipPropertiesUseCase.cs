using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetTimelineClipPropertiesUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly ITimelineOperations _operations;

        public GetTimelineClipPropertiesUseCase(IMainThreadDispatcher dispatcher, ITimelineOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetTimelineClipPropertiesResponse> ExecuteAsync(int instanceId, string assetPath,
            int trackIndex, int clipIndex, CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetClipProperties(instanceId, assetPath, trackIndex, clipIndex),
                cancellationToken);
        }
    }
}
