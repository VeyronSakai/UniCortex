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

        public async Task<SetGameViewSizeResponse> ExecuteAsync(int index, CancellationToken cancellationToken)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SetGameViewSize(index), cancellationToken);
            return new SetGameViewSizeResponse(true);
        }
    }
}
