using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetGameViewSizeUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorWindowOperations _operations;

        public SetGameViewSizeUseCase(IMainThreadDispatcher dispatcher, IEditorWindowOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<SetGameViewSizeResponse> ExecuteAsync(SetGameViewSizeRequest request,
            CancellationToken cancellationToken)
        {
            if (request.index >= 0)
            {
                await _dispatcher.RunOnMainThreadAsync(
                    () => _operations.SetGameViewSizeByIndex(request.index), cancellationToken);
            }
            else
            {
                await _dispatcher.RunOnMainThreadAsync(
                    () => _operations.SetGameViewSize(request.width, request.height), cancellationToken);
            }

            return new SetGameViewSizeResponse(true);
        }
    }
}
