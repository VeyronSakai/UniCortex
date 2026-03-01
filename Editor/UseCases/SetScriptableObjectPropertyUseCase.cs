using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetScriptableObjectPropertyUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IScriptableObjectOperations _operations;

        public SetScriptableObjectPropertyUseCase(IMainThreadDispatcher dispatcher, IScriptableObjectOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(string assetPath, string propertyPath, string value,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SetProperty(assetPath, propertyPath, value), cancellationToken);
        }
    }
}
