using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetProfilerStatusUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IProfilerOperations _operations;

        public GetProfilerStatusUseCase(IMainThreadDispatcher dispatcher, IProfilerOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetProfilerStatusResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            return await _dispatcher.RunOnMainThreadAsync(_operations.GetStatus, cancellationToken);
        }
    }
}
