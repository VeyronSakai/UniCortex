using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetScriptableObjectInfoUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IScriptableObjectOperations _operations;

        public GetScriptableObjectInfoUseCase(IMainThreadDispatcher dispatcher, IScriptableObjectOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<ScriptableObjectInfoResponse> ExecuteAsync(string assetPath,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetInfo(assetPath), cancellationToken);
        }
    }
}
