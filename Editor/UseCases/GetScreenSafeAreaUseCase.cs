using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetScreenSafeAreaUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPlayModeViewOperations _operations;

        public GetScreenSafeAreaUseCase(IMainThreadDispatcher dispatcher, IPlayModeViewOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public Task<GetScreenSafeAreaResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            return _dispatcher.RunOnMainThreadAsync(() => _operations.GetScreenSafeArea(), cancellationToken);
        }
    }
}
