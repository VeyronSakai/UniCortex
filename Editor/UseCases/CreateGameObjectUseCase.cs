using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class CreateGameObjectUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IGameObjectOperations _operations;

        public CreateGameObjectUseCase(IMainThreadDispatcher dispatcher, IGameObjectOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<CreateGameObjectResponse> ExecuteAsync(string name, string primitive,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.Create(name, primitive), cancellationToken);
        }
    }
}
