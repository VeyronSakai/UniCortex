using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetGameObjectInfoUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IGameObjectOperations _operations;

        public GetGameObjectInfoUseCase(IMainThreadDispatcher dispatcher, IGameObjectOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GameObjectInfoResponse> ExecuteAsync(int instanceId,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetInfo(instanceId), cancellationToken);
        }
    }
}
