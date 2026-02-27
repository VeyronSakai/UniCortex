using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class ModifyGameObjectUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IGameObjectOperations _operations;

        public ModifyGameObjectUseCase(IMainThreadDispatcher dispatcher, IGameObjectOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(int instanceId, string name, bool? activeSelf, string tag, int? layer,
            int? parentInstanceId, CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.Modify(instanceId, name, activeSelf, tag, layer, parentInstanceId),
                cancellationToken);
        }
    }
}
