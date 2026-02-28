using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class AddComponentUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IComponentOperations _operations;

        public AddComponentUseCase(IMainThreadDispatcher dispatcher, IComponentOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(int instanceId, string componentType,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.AddComponent(instanceId, componentType), cancellationToken);
        }
    }
}
