using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetPlayModeViewTypeUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPlayModeViewOperations _operations;

        public GetPlayModeViewTypeUseCase(IMainThreadDispatcher dispatcher, IPlayModeViewOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetPlayModeViewTypeResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            var viewType = await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetViewType(), cancellationToken);
            return new GetPlayModeViewTypeResponse(viewType);
        }
    }
}
