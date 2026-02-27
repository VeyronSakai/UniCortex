using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class FindGameObjectsUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IGameObjectOperations _operations;

        public FindGameObjectsUseCase(IMainThreadDispatcher dispatcher, IGameObjectOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<List<GameObjectBasicInfo>> ExecuteAsync(string name, string tag, string componentType,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.Find(name, tag, componentType), cancellationToken);
        }
    }
}
