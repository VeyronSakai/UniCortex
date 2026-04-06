using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class StartMovieRecordingUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IMovieRecordingOperations _operations;

        public StartMovieRecordingUseCase(IMainThreadDispatcher dispatcher, IMovieRecordingOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(int index, int fps = RecorderFps.Default,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.StartMovieRecording(index, fps),
                cancellationToken);
        }
    }
}
