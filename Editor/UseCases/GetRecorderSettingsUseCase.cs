using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetRecorderSettingsUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IRecordingOperations _operations;

        public GetRecorderSettingsUseCase(IMainThreadDispatcher dispatcher, IRecordingOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetRecorderSettingsResponse> ExecuteAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetRecorderSettings(), cancellationToken);
        }
    }
}
