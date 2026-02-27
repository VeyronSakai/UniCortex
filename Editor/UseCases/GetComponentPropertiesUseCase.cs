using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetComponentPropertiesUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IComponentOperations _operations;

        public GetComponentPropertiesUseCase(IMainThreadDispatcher dispatcher, IComponentOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<ComponentPropertiesResponse> ExecuteAsync(int instanceId, string componentType,
            int componentIndex, CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetProperties(instanceId, componentType, componentIndex), cancellationToken);
        }
    }
}
