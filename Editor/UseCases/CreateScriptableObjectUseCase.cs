using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class CreateScriptableObjectUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IScriptableObjectOperations _operations;

        public CreateScriptableObjectUseCase(IMainThreadDispatcher dispatcher,
            IScriptableObjectOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<CreateScriptableObjectResponse> ExecuteAsync(string typeName, string assetPath,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.Create(typeName, assetPath), cancellationToken);
        }
    }
}
