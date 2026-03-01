using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class ExecuteMenuItemUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IMenuItemOperations _operations;

        public ExecuteMenuItemUseCase(IMainThreadDispatcher dispatcher, IMenuItemOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<bool> ExecuteAsync(string menuPath, CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.ExecuteMenuItem(menuPath), cancellationToken);
        }
    }
}
