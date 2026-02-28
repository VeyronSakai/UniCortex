using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class RemoveComponentUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IComponentOperations _operations;

        public RemoveComponentUseCase(IMainThreadDispatcher dispatcher, IComponentOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(int instanceId, string componentType, int componentIndex,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.RemoveComponent(instanceId, componentType, componentIndex), cancellationToken);
        }
    }
}
