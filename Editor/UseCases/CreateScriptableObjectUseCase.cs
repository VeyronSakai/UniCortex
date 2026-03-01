using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class CreateScriptableObjectUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IScriptableObjectOperations _operations;

        public CreateScriptableObjectUseCase(IMainThreadDispatcher dispatcher, IScriptableObjectOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(string type, string assetPath,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.Create(type, assetPath), cancellationToken);
        }
    }
}
