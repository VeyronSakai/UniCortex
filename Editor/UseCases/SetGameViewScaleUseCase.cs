using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetGameViewScaleUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorWindowOperations _operations;

        public SetGameViewScaleUseCase(IMainThreadDispatcher dispatcher, IEditorWindowOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<SetGameViewScaleResponse> ExecuteAsync(float scale, CancellationToken cancellationToken)
        {
            var applied = await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SetGameViewScale(scale), cancellationToken);
            return new SetGameViewScaleResponse(true, applied);
        }
    }
}
