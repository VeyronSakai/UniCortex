using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetAnimationCurvesUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IAnimationClipOperations _operations;

        public GetAnimationCurvesUseCase(IMainThreadDispatcher dispatcher, IAnimationClipOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetAnimationCurvesResponse> ExecuteAsync(string assetPath,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetCurves(assetPath), cancellationToken);
        }
    }
}
