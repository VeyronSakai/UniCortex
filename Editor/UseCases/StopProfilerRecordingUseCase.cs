using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class StopProfilerRecordingUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IProfilerOperations _operations;

        public StopProfilerRecordingUseCase(IMainThreadDispatcher dispatcher, IProfilerOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(_operations.StopRecording, cancellationToken);
        }
    }
}
