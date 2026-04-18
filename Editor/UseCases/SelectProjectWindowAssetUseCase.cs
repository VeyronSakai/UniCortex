using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SelectProjectWindowAssetUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IProjectWindowOperations _operations;

        public SelectProjectWindowAssetUseCase(IMainThreadDispatcher dispatcher, IProjectWindowOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(string assetPath, CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SelectAsset(assetPath), cancellationToken);
        }
    }
}
