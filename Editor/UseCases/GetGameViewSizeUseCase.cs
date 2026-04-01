using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetGameViewSizeUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorWindowOperations _operations;

        public GetGameViewSizeUseCase(IMainThreadDispatcher dispatcher, IEditorWindowOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetGameViewSizeResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            var (width, height) = await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetGameViewSize(), cancellationToken);
            return new GetGameViewSizeResponse(width, height);
        }
    }
}
