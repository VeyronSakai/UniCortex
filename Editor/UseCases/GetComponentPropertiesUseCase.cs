using System;
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

        public async Task<GetComponentPropertiesResponse> ExecuteAsync(int instanceId, string componentType,
            string assemblyName, int componentIndex, string propertyPath = null, int? depth = null,
            CancellationToken cancellationToken = default)
        {
            if (depth < 0)
            {
                throw new ArgumentException($"depth must be 0 or greater (got {depth}).");
            }

            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetProperties(instanceId, componentType, assemblyName, componentIndex, propertyPath,
                    depth),
                cancellationToken);
        }
    }
}
