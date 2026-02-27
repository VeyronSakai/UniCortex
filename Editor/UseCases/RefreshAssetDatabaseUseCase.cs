using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class RefreshAssetDatabaseUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IAssetOperations _operations;

        public RefreshAssetDatabaseUseCase(IMainThreadDispatcher dispatcher, IAssetOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() => _operations.Refresh(), cancellationToken);
        }
    }
}
