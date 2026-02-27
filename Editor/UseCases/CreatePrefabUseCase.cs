using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class CreatePrefabUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPrefabOperations _operations;

        public CreatePrefabUseCase(IMainThreadDispatcher dispatcher, IPrefabOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(int instanceId, string assetPath,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.CreatePrefab(instanceId, assetPath), cancellationToken);
        }
    }
}
