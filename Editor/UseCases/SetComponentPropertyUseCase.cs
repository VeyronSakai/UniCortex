using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetComponentPropertyUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IComponentOperations _operations;

        public SetComponentPropertyUseCase(IMainThreadDispatcher dispatcher, IComponentOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(int instanceId, string componentType, string propertyPath, string value,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SetProperty(instanceId, componentType, propertyPath, value), cancellationToken);
        }
    }
}
