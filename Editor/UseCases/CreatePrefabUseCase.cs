using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

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

        public async Task<CreatePrefabResponse> ExecuteAsync(int instanceId, string assetPath, bool connect = false,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.CreatePrefab(instanceId, assetPath, connect), cancellationToken);
        }
    }
}
