using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class StopRecordingUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IRecordingOperations _operations;

        public StopRecordingUseCase(IMainThreadDispatcher dispatcher, IRecordingOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.StopRecording(), cancellationToken);
        }
    }
}
