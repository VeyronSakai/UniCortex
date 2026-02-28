using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class InstantiatePrefabUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPrefabOperations _operations;

        public InstantiatePrefabUseCase(IMainThreadDispatcher dispatcher, IPrefabOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<InstantiatePrefabResponse> ExecuteAsync(string assetPath,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.InstantiatePrefab(assetPath), cancellationToken);
        }
    }
}
