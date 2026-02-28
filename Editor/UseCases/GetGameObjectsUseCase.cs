using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetGameObjectsUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IGameObjectOperations _operations;

        public GetGameObjectsUseCase(IMainThreadDispatcher dispatcher, IGameObjectOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<List<GameObjectSearchResult>> ExecuteAsync(string query,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.Get(query), cancellationToken);
        }
    }
}
