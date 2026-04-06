using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetRecorderListUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IMovieRecordingOperations _operations;

        public GetRecorderListUseCase(IMainThreadDispatcher dispatcher, IMovieRecordingOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<RecorderEntry[]> ExecuteAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetRecorderList(), cancellationToken);
        }
    }
}
