using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class StopMovieRecordingUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IMovieRecordingOperations _operations;

        public StopMovieRecordingUseCase(IMainThreadDispatcher dispatcher, IMovieRecordingOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.StopMovieRecording(), cancellationToken);
        }
    }
}
