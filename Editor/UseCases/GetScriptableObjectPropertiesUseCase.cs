using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetScriptableObjectPropertiesUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IScriptableObjectOperations _operations;

        public GetScriptableObjectPropertiesUseCase(IMainThreadDispatcher dispatcher,
            IScriptableObjectOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetScriptableObjectPropertiesResponse> ExecuteAsync(string assetPath,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetProperties(assetPath), cancellationToken);
        }
    }
}
