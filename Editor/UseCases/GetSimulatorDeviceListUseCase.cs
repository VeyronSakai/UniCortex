using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetSimulatorDeviceListUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPlayModeViewOperations _operations;

        public GetSimulatorDeviceListUseCase(IMainThreadDispatcher dispatcher, IPlayModeViewOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public Task<GetSimulatorDeviceListResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            return _dispatcher.RunOnMainThreadAsync(() => _operations.GetSimulatorDeviceList(), cancellationToken);
        }
    }
}
