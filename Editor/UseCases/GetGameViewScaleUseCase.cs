using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetGameViewScaleUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorWindowOperations _operations;

        public GetGameViewScaleUseCase(IMainThreadDispatcher dispatcher, IEditorWindowOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetGameViewScaleResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            var (scale, minScale, maxScale) = await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetGameViewScale(), cancellationToken);
            return new GetGameViewScaleResponse(scale, minScale, maxScale);
        }
    }
}
