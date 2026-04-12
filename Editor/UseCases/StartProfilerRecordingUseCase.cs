using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class StartProfilerRecordingUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IProfilerOperations _operations;

        public StartProfilerRecordingUseCase(IMainThreadDispatcher dispatcher, IProfilerOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(bool profileEditor = false, CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() => _operations.StartRecording(profileEditor), cancellationToken);
        }
    }
}
