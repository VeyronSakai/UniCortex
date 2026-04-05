using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class RemoveMovieRecorderUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IMovieRecordingOperations _operations;

        public RemoveMovieRecorderUseCase(IMainThreadDispatcher dispatcher, IMovieRecordingOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(int index, CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.RemoveMovieRecorder(index), cancellationToken);
        }
    }
}
