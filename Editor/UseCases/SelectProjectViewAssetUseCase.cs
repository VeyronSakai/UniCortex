using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SelectProjectViewAssetUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IProjectViewOperations _operations;

        public SelectProjectViewAssetUseCase(IMainThreadDispatcher dispatcher, IProjectViewOperations operations)
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
