using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class CreateAssetUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IAssetOperations _operations;

        public CreateAssetUseCase(IMainThreadDispatcher dispatcher, IAssetOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(string type, string assetPath,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.CreateAsset(type, assetPath), cancellationToken);
        }
    }
}
