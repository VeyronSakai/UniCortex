using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetMovieRecorderListUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IMovieRecordingOperations _operations;

        public GetMovieRecorderListUseCase(IMainThreadDispatcher dispatcher, IMovieRecordingOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<MovieRecorderEntry[]> ExecuteAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetMovieRecorderList(), cancellationToken);
        }
    }
}
