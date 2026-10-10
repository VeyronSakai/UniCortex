using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetActivePlatformUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IBuildTargetOperations _operations;

        public GetActivePlatformUseCase(IMainThreadDispatcher dispatcher, IBuildTargetOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetActivePlatformResponse> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var activeBuildTarget =
                await _dispatcher.RunOnMainThreadAsync(() => _operations.GetActiveBuildTarget(), cancellationToken);
            return new GetActivePlatformResponse(activeBuildTarget);
        }
    }
}
