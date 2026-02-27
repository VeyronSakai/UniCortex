using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetAssetPropertyUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IAssetOperations _operations;

        public SetAssetPropertyUseCase(IMainThreadDispatcher dispatcher, IAssetOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(string assetPath, string propertyPath, string value,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SetProperty(assetPath, propertyPath, value), cancellationToken);
        }
    }
}
