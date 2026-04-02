using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetGameViewSizeListUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorWindowOperations _operations;

        public GetGameViewSizeListUseCase(IMainThreadDispatcher dispatcher, IEditorWindowOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetGameViewSizeListResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetGameViewSizeList(), cancellationToken);
        }
    }
}
